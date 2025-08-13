using System.Diagnostics.CodeAnalysis;
using Lira.Common.Extensions;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Parsing;

class TokenParser
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

    public IReadOnlyList<Token> Parse(PatternParts input)
    {
        var openOperators = new Stack<Token.Operator>();

        var result = new List<Token>();
        var iterator = new PatternPartsIterator(input);

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

                if (openOperators.Count == 0)
                    result.Add(closedOperator);
            }
            // значит попытка объявить оператор или элемент оператора
            else
            {
                // значит это попытка определения оператора
                if (openOperators.Count == 0)
                {
                    if (!TryGetOperatorDefinition(iterator, out var operatorDefinition))
                        throw new TokenParsingException($"Unknown operator '{PopName(iterator)}'");

                    var parameters = PopParameters(operatorDefinition.Name, iterator);

                    iterator.MoveToNewlineAndPop();

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
                    if (TryGetOperatorDefinition(iterator, out var operatorDefinition))
                    {
                        var parameters = PopParameters(operatorDefinition.Name, iterator);
                        var current = openOperators.Peek();

                        iterator.MoveToNewlineAndPop();

                        var @operator = new Token.Operator(operatorDefinition, parameters);

                        current.AddContent(@operator);
                        openOperators.Push(@operator);
                    }
                    // иначе попытка объявления элемента внутри текущего оператора
                    else
                    {
                        var current = openOperators.Peek();
                        var element = GetAllowedChildElementDefinition(current, iterator);

                        var elemParameters = PopParameters(element.Name, iterator);

                        iterator.MoveToNewlineAndPop();

                        current.AddChildElement(new Token.Operator.OperatorElement(element, elemParameters));
                    }
                }
            }
        }

        if (openOperators.Count > 0)
        {
            var @operator = openOperators.Peek();
            throw new TokenParsingException(
                $"Operator @{@operator.Definition.Name + @operator.Parameters} must be closed with an @end");
        }

        if (iterator.HasNext())
        {
            iterator.MoveToEnd();
            AddContent(iterator.PopIncludeCurrent());
        }

        foreach (var token in result)
        {
            if (token is Token.Operator op)
            {
                ClearWhitespaces(op);
            }
        }

        return result;

        void AddContent(PatternParts content)
        {
            if (content.Count == 0)
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

    private static AllowedChildElementDefinition GetAllowedChildElementDefinition(Token.Operator current,
        PatternPartsIterator iterator)
    {
        var allowedChildElems = current.Definition.AllowedChildElements.Select(x => x.Value)
            .OrderByDescending(x => x.Name.Length);

        foreach (var elem in allowedChildElems)
        {
            if (iterator.NextIs(elem.Name))
            {
                iterator.MoveToEnd(elem.Name);
                iterator.PopIncludeCurrent();
                return elem;
            }
        }

        throw new TokenParsingException(
            $"Unknown element @{PopName(iterator)} for operator {current.Definition.Name}");
    }

    private bool TryGetOperatorDefinition(PatternPartsIterator iterator,
        [MaybeNullWhen(false)] out OperatorDefinition definition)
    {
        definition = null;
        foreach (var (operatorName, @operator) in _operatorDefinitions)
        {
            if (iterator.NextIs(operatorName))
            {
                iterator.MoveToEnd(operatorName);
                iterator.PopIncludeCurrent();
                definition = @operator;
                return true;
            }
        }

        return false;
    }

    private static void ClearWhitespaces(Token.Operator op)
    {
        op.Content = Trim(op.Content);

        foreach (var elem in op.Elements)
        {
            elem.Content = Trim(elem.Content);
        }
    }

    private static List<Token> Trim(IReadOnlyList<Token> content)
    {
        var trimmed = new List<Token>();

        if (content.Count > 1)
        {
            var first = content.First();

            if (first is Token.StaticData sdFirst)
            {
                var newContent = sdFirst.Content.Split("\n").TrimStartEmptyLines().JoinWithNewLine();

                if (newContent.Count > 0)
                {
                    sdFirst.Content = newContent;
                    trimmed.Add(sdFirst);
                }
            }

            for (int i = 1; i < content.Count - 1; i++)
            {
                trimmed.Add(content[i]);
            }

            var last = content.Last();
            if (last is Token.StaticData sdLast)
            {
                var newContent = sdLast.Content.Split("\n").TrimEndEmptyLines().JoinWithNewLine();

                if (newContent.Count > 0)
                {
                    sdLast.Content = newContent;
                    trimmed.Add(sdLast);
                }
            }
        }

        if (content.Count == 1)
        {
            var single = content.First();
            if (single is Token.StaticData sd)
            {
                var lines = sd.Content.Split("\n").TrimEmptyLines().ToArray();
                var ifSingleLine = lines.TrimEndIfSingleLine().ToArray();
                var newContent = ifSingleLine.JoinWithNewLine();

                if (newContent.Count > 0)
                {
                    sd.Content = newContent;
                    trimmed.Add(sd);
                }
            }
            else
            {
                trimmed.Add(single);
            }
        }

        var indentCounts = content.OfType<Token.StaticData>()
            .SelectMany(x => x.Content.Split("\n"))
            .TrimEmptyLines()
            .SelectMany(x => x)
            .OfType<PatternPart.Static>()
            .Where(x => x.Value.Length != 0)
            .Select(x => x.Value.GetCountWhitespacesStart())
            .ToArray();

        var minIndent = indentCounts.Length == 0 ? 0 : indentCounts.Min();

        var result = new List<Token>();
        foreach (var token in trimmed)
        {
            if (token is Token.Operator o)
            {
                ClearWhitespaces(o);
                result.Add(o);
            }

            if (token is Token.StaticData st)
            {
                var newContent = st.Content.Split("\n").Select(x => x.Substring(minIndent)).JoinWithNewLine();
                if (newContent.Count > 0)
                {
                    st.Content = newContent;
                    result.Add(st);
                }
            }
        }

        return result;
    }

    private static string PopName(PatternPartsIterator iterator)
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

        var maybeName = iterator.PopExcludeCurrent().GetStaticValue();

        //  символ @ стоит в самом конце
        if (maybeName == "@")
            throw new TokenParsingException("End symbol @ must be escaped as @@ or contains operator name");

        return maybeName.TrimStart('@');
    }

    private static OperatorParameters? PopParameters(string name, PatternPartsIterator iterator)
    {
        var pair = GetPair(iterator.Current);

        if (pair != null)
            return PopParametersAfterBrace(name, iterator, pair.Value.End);

        if (iterator.MoveTo(currentPredicate: c => Chars.StartParams.Pairs.Any(x => x.Start == c),
                available: char.IsWhiteSpace))
        {
            pair = GetPair(iterator.Current) ??
                   throw new TokenParsingException($"Unknown character '{iterator.Current}'");
            iterator.PopExcludeCurrent();
            return PopParametersAfterBrace(name, iterator, pair.Value.End);
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

    static OperatorParameters? PopParametersAfterBrace(string name, PatternPartsIterator iterator, char valueEnd)
    {
        int braces = 0;

        var sourceIterator = iterator.Clone();
        while (iterator.MoveNext())
        {
            if (iterator.Current == valueEnd && braces == 0)
            {
                return new OperatorParameters(iterator.PopIncludeCurrent().GetStaticValue());
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

        // возвращаемся назад к определению оператора или его элемента, чтобы вывести информативное сообщение об ошибке
        sourceIterator.MoveBackTo('@');
        sourceIterator.SavePopPosition();
        sourceIterator.MoveToEnd();
        var pars = sourceIterator.PopIncludeCurrent();

        throw new TokenParsingException(
            $"Missing closing symbol '{valueEnd}' when defining @{name} parameters: '{pars}'");
    }
}