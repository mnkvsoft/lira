using Lira.Common.Extensions;

namespace Lira.Domain.Configuration.Parsing;

public class TokenParser
{
    static class Chars
    {
        public static class StartParams
        {
            public static readonly IReadOnlyCollection<(char Start, char End)> Pairs =
            [
                ('(', ')'),
                (':', '\n')
            ];
        }
    }

    private readonly Dictionary<string, OperatorDefinition> _operatorDefinitions;

    public TokenParser(IEnumerable<OperatorDefinition> operatorDefinitions)
    {
        _operatorDefinitions = operatorDefinitions.ToDictionary(
            x => x.Name,
            x => x);
    }

    public IReadOnlyList<Token> Parse(string input)
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
                iterator.PopIncludeCurrent();
                if (!openOperators.TryPop(out var closedOperator))
                    throw new TokenParsingException("For @end operator missing the open operator");

                if(openOperators.Count == 0)
                    result.Add(closedOperator);
            }
            // значит попытка объявить оператор или элемент оператора
            else
            {
                var beforeName = iterator.GetAllFromStart();
                string name = PopName(iterator);
                var parameters = PopParameters(iterator);

                // значит это попытка определения оператора
                if (openOperators.Count == 0)
                {
                    if (!_operatorDefinitions.TryGetValue(name, out OperatorDefinition? operatorDefinition))
                        throw new TokenParsingException($"Unknown operator '{name}'");

                    int newLineIndent = beforeName.GetCountWhitespacesEnd();

                    iterator.MoveToNewlineAndPop();

                    var @operator = new Token.Operator(operatorDefinition, parameters, newLineIndent);
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
                        var current = openOperators.Peek();

                        int newLineIndent = current.Content.Count == 0 ? current.NewLineIndent : beforeName.GetCountWhitespacesEnd();

                        iterator.MoveToNewlineAndPop();

                        var @operator = new Token.Operator(operatorDefinition, parameters, newLineIndent);

                        current.AddContent(@operator);
                        openOperators.Push(@operator);
                    }
                    // иначе поптыка объявления элемента внутри текущего оператора
                    else
                    {
                        var current = openOperators.Peek();
                        var allowedElements = current.Definition.AllowedChildElements;

                        if (!allowedElements.TryGetValue(name, out var options))
                            throw new TokenParsingException(
                                $"Unknown element @'{name}' for operator {current.Definition.Name}");

                        iterator.MoveToNewlineAndPop();

                        current.AddChildElement(new Token.Operator.OperatorElement(options, parameters, current.NewLineIndent));
                    }
                }
            }
        }

        if (openOperators.Count > 0)
            throw new TokenParsingException($"Operator @{openOperators.Peek()} must be closed with an @end");

        if (iterator.HasNext())
        {
            iterator.MoveToEnd();
            AddContent(iterator.PopIncludeCurrent());
        }

        return result;

        void AddContent(string? content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return;

            if (openOperators.Count == 0)
            {
                result.Add(new Token.StaticData(content));
            }
            else
            {
                var current = openOperators.Peek();
                current.AddStaticContent(content);
            }
        }
    }

    private static string PopName(StringIterator iterator)
    {
        // пытаемся получить имя
        while (iterator.MoveNext())
        {
            if (char.IsWhiteSpace(iterator.Current))
            {
                // закончили ввод имени
                break;
            }

            bool isEnd = false;
            foreach (var pair in Chars.StartParams.Pairs)
            {
                if (pair.Start == iterator.Current)
                    // закончили ввод имени
                    isEnd = true;
            }

            if (isEnd)
                break;

            if (!char.IsLetter(iterator.Current))
                throw new TokenParsingException(
                    $"Unexpected character '{iterator.Current}' in operator name: {iterator.Peek()}");
        }

        var maybeName = iterator.PopExcludeCurrent() ?? throw new NullReferenceException("Name cannot be null");

        //  символ @ стоит в самом конце
        if (maybeName == "@")
            throw new TokenParsingException("End symbol @ must be escaped as @@ or contains operator name");

        return maybeName.TrimStart('@');
    }

    private static string? PopParameters(StringIterator iterator)
    {
        var pair = GetPair(iterator.Current);

        if (pair != null)
            return PopParametersAfterBrace(iterator, pair.Value.End);

        if (iterator.MoveTo(currentPredicate: c => Chars.StartParams.Pairs.Any(x => x.Start == c),
                available: char.IsWhiteSpace))
        {
            pair = GetPair(iterator.Current) ??
                   throw new TokenParsingException($"Unknown character '{iterator.Current}'");
            iterator.PopExcludeCurrent();
            return PopParametersAfterBrace(iterator, pair.Value.End);
        }

        return null;

        (char Start, char End)? GetPair(char current)
        {
            (char Start, char End)? valueTuple = null;

            foreach (var p in Chars.StartParams.Pairs)
            {
                if (p.Start == current)
                {
                    valueTuple = p;
                    break;
                }
            }

            return valueTuple;
        }
    }

    static string? PopParametersAfterBrace(StringIterator iterator, char valueEnd)
    {
        int braces = 0;
        while (iterator.MoveNext())
        {
            if (iterator.Current == valueEnd && braces == 0)
            {
                break;
            }

            switch (iterator.Current)
            {
                case ')':
                    braces--;
                    break;
                case '(':
                    braces++;
                    break;
            }
        }

        return iterator.PopIncludeCurrent();
    }
}