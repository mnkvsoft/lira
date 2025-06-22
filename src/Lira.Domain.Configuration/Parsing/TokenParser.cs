namespace Lira.Domain.Configuration.Parsing;

public class TokenParser
{
    private readonly Dictionary<string, OperatorDefinition> _operatorDefinitions;

    public TokenParser(IEnumerable<OperatorDefinition> operatorDefinitions)
    {
        _operatorDefinitions = operatorDefinitions.ToDictionary(
            x => x.Name,
            x => x);
    }

    public List<Token> Parse(string input)
    {
        var openOperators = new Stack<Token.Operator>();

        var result = new List<Token>();
        var iterator = new StringIterator(input);

        while (iterator.MoveTo('@'))
        {
            AddContent(iterator.PopExcludeCurrent());

            // это экранирование
            if (iterator.NextIs('@'))
            {
                iterator.MoveToWhereNext(c => c != '@');
                AddContent(iterator.PopExcludeCurrent());
            }
            else if (iterator.NextIs("end"))
            {
                iterator.MoveToEnd("end");
                iterator.PopExcludeCurrent();
                if (!openOperators.TryPop(out var closedOperator))
                    throw new TokenParsingException("For @end operator missing the open operator");

                result.Add(closedOperator);
            }
            // значит попытка объявить оператор или элемент оператора
            else
            {
                // пытаемся получить имя
                while (iterator.MoveNext())
                {
                    if (iterator.Current == '(' || char.IsWhiteSpace(iterator.Current))
                    {
                        // закончили ввод имени
                        break;
                    }

                    if (!char.IsLetter(iterator.Current))
                        throw new TokenParsingException(
                            $"Unexpected character '{iterator.Current}' in operator name: {iterator.Peek()}");
                }

                var maybeName = iterator.PopExcludeCurrent() ?? throw new NullReferenceException("Name cannot be null");

                //  символ @ стоит в самом конце
                if (maybeName == "@")
                    throw new TokenParsingException("End symbol @ must be escaped as @@ or contains operator name");

                var name = maybeName.TrimStart('@');
                var parameters = PopParameters(iterator);
                iterator.PopIncludeCurrent();

                // значит это попытка определения оператора
                if (openOperators.Count == 0)
                {
                    if (!_operatorDefinitions.TryGetValue(name, out OperatorDefinition? operatorDefinition))
                        throw new TokenParsingException($"Unknown operator '{name}'");

                    var @operator = new Token.Operator(operatorDefinition, parameters);
                    if (operatorDefinition.WithBody)
                    {
                        openOperators.Push(@operator);
                    }
                    else
                    {
                        result.Add(@operator);
                    }
                }
                else
                {
                    // начинается новый оператор внутри текущего
                    if (_operatorDefinitions.TryGetValue(name, out OperatorDefinition? operatorDefinition))
                    {
                        var @operator = new Token.Operator(operatorDefinition, parameters);
                        var current = openOperators.Peek();
                        if (current.Children.Count == 0)
                        {
                            current.AddContent(@operator);
                        }
                        else
                        {
                            var lastItem = current.Children.Last();
                            lastItem.AddContent(@operator);
                        }

                        openOperators.Push(@operator);
                    }
                    // иначе поптыка объявления элемента внутри текущего оператора
                    else
                    {
                        var current = openOperators.Peek();
                        var allowedElements = current.Definition.AllowedChildElements;

                        if (!allowedElements.TryGetValue(name, out var options))
                            throw new TokenParsingException(
                                $"Unknown element '{maybeName}' for operator {current.Definition.Name}");

                        current.AddChildElement(new Token.Operator.OperatorElement(options, parameters));
                    }
                }
            }
        }

        if(openOperators.Count > 0)
            throw new TokenParsingException($"Operator @{openOperators.Peek()} must be closed with an @end");

        if (iterator.HasNext())
        {
            iterator.MoveToEnd();
            AddContent(iterator.PopExcludeCurrent());
        }

        return result;

        void AddContent(string? content)
        {
            if(content == null)
                return;

            var staticData = new Token.StaticData(content);
            if (openOperators.Count == 0)
            {
                result.Add(staticData);
            }
            else
            {
                var current = openOperators.Peek();
                if (current.Children.Count == 0)
                {
                    current.AddContent(staticData);
                }
                else
                {
                    var lastItem = current.Children.Last();
                    lastItem.AddContent(staticData);
                }
            }
        }
    }

    private static string? PopParameters(StringIterator iterator)
    {
        if (iterator.Current == '(')
            return PopParametersAfterBrace(iterator);

        if (iterator.MoveTo('(', available: char.IsWhiteSpace))
        {
            iterator.PopExcludeCurrent();
            return PopParametersAfterBrace(iterator);
        }

        return null;
    }

    static string PopParametersAfterBrace(StringIterator iterator)
    {
        int braces = 0;
        while (iterator.MoveNext())
        {
            if (iterator.Current == ')')
            {
                if (braces == 0)
                    break;
                braces--;
            }
            else if (iterator.Current == '(')
            {
                braces++;
            }
        }

        return iterator.PopExcludeCurrent();
    }
}
