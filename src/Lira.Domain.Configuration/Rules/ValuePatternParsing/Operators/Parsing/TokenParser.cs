using System.Diagnostics.CodeAnalysis;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Parsing;

class TokenParser
{
    static class Chars
    {
        public static class StartParams
        {
            public const char Full = '(';
            public const char SingleLine = ':';

            public static IReadOnlyCollection<char> List = [Full, SingleLine];
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
            .Where(x => x.Count != 0)
            .Select(x => x.GetCountWhitespacesStart())
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

            if(Chars.StartParams.List.Contains(iterator.Current))
                break;

            if (!char.IsLetter(iterator.Current))
                throw new TokenParsingException(
                    $"Unexpected character '{iterator.Current}' in operator name: {iterator.Peek()}");
        }

        var maybeName = iterator.PopExcludeCurrent().GetSingleStaticValue();

        //  символ @ стоит в самом конце
        if (maybeName == "@")
            throw new TokenParsingException("End symbol @ must be escaped as @@ or contains operator name");

        return maybeName.TrimStart('@');
    }

    private static OperatorParameters? PopParameters(string name, PatternPartsIterator iterator)
    {
        if (iterator.Current != Chars.StartParams.SingleLine || iterator.Current != Chars.StartParams.Full)
        {
            if (!iterator.MoveTo(currentPredicate: c => Chars.StartParams.List.Contains(c),
                    available: char.IsWhiteSpace))
                return null;
        }

        if(iterator.Current == Chars.StartParams.SingleLine)
            return PopParametersAfterBraceForSingleLine(name, iterator);

        if(iterator.Current == Chars.StartParams.Full)
            return PopParametersAfterBraceForFull(name, iterator);

        throw new TokenParsingException($"Unknown character '{iterator.Current}'");
    }

    static OperatorParameters? PopParametersAfterBraceForSingleLine(string name, PatternPartsIterator iterator)
    {
        int braces = 0;

        var sourceIterator = iterator.Clone();

        // убираем стартовый символ параметров
        iterator.PopIncludeCurrent();
        while (iterator.MoveNext())
        {
            if (iterator.Current == '\n' && braces == 0)
            {
                var parts = iterator.PopExcludeCurrent();
                // убираем завершающий символ параметров
                iterator.PopIncludeCurrent();

                var value = parts.Count == 0 ? null : parts.GetSingleStaticValue();
                if (string.IsNullOrWhiteSpace(value))
                    return null;

                return new OperatorParameters(OperatorParametersType.SingleLine, value);
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
            $"Missing closing symbol '\\\n' when defining @{name} parameters: '{pars}'");
    }

    static OperatorParameters? PopParametersAfterBraceForFull(string name, PatternPartsIterator iterator)
    {
        int braces = 0;

        var sourceIterator = iterator.Clone();

        // убираем стартовый символ параметров
        iterator.PopIncludeCurrent();

        bool isString = false;
        while (iterator.MoveNext())
        {
            if (isString && iterator.Current == '\\')
            {
                if (iterator.HasNext())
                {
                    iterator.MoveNext();
                    continue;
                }

                throw new TokenParsingException(
                    $"Missing closing symbol '\"' for string when defining @{name} parameters: '{sourceIterator.PopIncludeCurrent()}'");
            }

            if (iterator.Current == '"')
            {
                if (isString)
                {
                    isString = false;
                    continue;
                }

                isString = true;
                continue;
            }

            if(isString)
                continue;

            if (iterator.Current == ')' && braces == 0)
            {
                var parts = iterator.PopExcludeCurrent();
                // убираем завершающий символ параметров
                iterator.PopIncludeCurrent();

                var value = parts.Count == 0 ? null : parts.GetSingleStaticValue();
                if (string.IsNullOrWhiteSpace(value))
                    return null;

                return new OperatorParameters(OperatorParametersType.Full, value);
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

        throw new TokenParsingException(
            $"Missing closing symbol ')' when defining @{name} parameters: '{sourceIterator.PopIncludeCurrent()}'");
    }
}