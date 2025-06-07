using Lira.Common.Exceptions;
using Lira.Common.Extensions;
using Lira.Domain.TextPart;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators;

static class OperatorParser
{
    class OperatorDataItemDraft
    {
        public required string Name { get; init; }
        public required  string Parameters { get;init; }
        public List<IObjectTextPart> Body { get; } = new();

    }

    public static OperatorData Parse(OperatorDraft draft, params string[] itemNames)
    {
        var tokens = Tokenize(draft.Content, itemNames);
        var (body, items) = GetOperatorData(tokens);
        return new OperatorData(draft.Name, draft.Parameters, body, items);
    }

    private static (IReadOnlyCollection<IObjectTextPart> Body, IReadOnlyCollection<OperatorDataItem>) GetOperatorData(IReadOnlyCollection<OperatorToken> tokens)
    {
        var body = new List<IObjectTextPart>();
        var items = new Stack<OperatorDataItemDraft>();

        foreach (var token in tokens)
        {
            if (items.Count == 0)
            {
                switch (token)
                {
                    case OperatorToken.OperatorItemDeclaration item:
                        items.Push(new OperatorDataItemDraft
                        {
                            Name = item.Name,
                            Parameters = item.Parameters,
                        });
                        break;
                    case OperatorToken.Static @static:
                        if(body.Count == 0 && string.IsNullOrWhiteSpace(@static.Value))
                            break;

                        body.Add(new StaticPart(@static.Value));
                        // if (string.IsNullOrWhiteSpace(@static.Value))
                        //     continue;
                        //
                        // throw new Exception("Before first @item operator cannot be other declarations. Declaration: " +
                        //                     @static.Value);
                        break;
                    case OperatorToken.Dynamic dynamic:
                        body.Add(dynamic.Parts);
                        break;
                    default:
                        throw new UnsupportedInstanceType(token);
                }
            }
            else
            {
                switch (token)
                {
                    case OperatorToken.OperatorItemDeclaration item:
                        items.Push(new OperatorDataItemDraft
                        {
                            Name = item.Name,
                            Parameters = item.Parameters,
                        });
                        break;
                    case OperatorToken.Static @static:
                        items.Peek().Body.Add(new StaticPart(@static.Value));
                        break;
                    case OperatorToken.Dynamic dynamic:
                        items.Peek().Body.Add(dynamic.Parts);
                        break;
                    default:
                        throw new UnsupportedInstanceType(token);
                }
            }
        }

        return (body, items.Select(i => new OperatorDataItem(i.Name, i.Parameters, i.Body)).ToArray());
    }

    abstract record OperatorToken
    {
        public record Static(string Value) : OperatorToken;

        public record Dynamic(IObjectTextPart Parts) : OperatorToken;

        public record OperatorItemDeclaration(string Name, string Parameters) : OperatorToken;
    }

    static IReadOnlyCollection<OperatorToken> Tokenize(IReadOnlyCollection<IObjectTextPart> parts,
        params string[] itemNames)
    {
        var result = new List<OperatorToken>();

        foreach (var part in parts)
        {
            if (part is not StaticPart @static)
            {
                result.Add(new OperatorToken.Dynamic(part));
            }
            else
            {
                var value = @static.Value;
                var positions = value.GetPositions(itemNames);

                if (positions.Count == 0)
                {
                    result.Add(new OperatorToken.Static(@static.Value));
                }
                else
                {
                    string beforePosition = value[..positions.First().Index];
                    if (beforePosition != string.Empty)
                    {
                        result.Add(new OperatorToken.Static(beforePosition));
                    }

                    for (int i = 0; i < positions.Count; i++)
                    {
                        var position = positions[i];
                        var isLast = i == positions.Count - 1;
                        var from = position.Index + position.Name.Length;
                        var to = (isLast ? value.Length : positions[i + 1].Index);
                        var betweenPositions = value[from..to];

                        var nl = Common.Constants.NewLine;
                        var parametersEndIndex = betweenPositions.IndexOf(nl,
                            StringComparison.OrdinalIgnoreCase);

                        if (parametersEndIndex == -1)
                            throw new Exception(
                                $"After defining operator {position} there should be a new line");

                        var parameters = betweenPositions[..parametersEndIndex];
                        var afterParameters = betweenPositions[(parametersEndIndex + nl.Length)..];

                        result.Add(new OperatorToken.OperatorItemDeclaration(position.Name, parameters));
                        result.Add(new OperatorToken.Static(afterParameters));
                    }
                }
            }
        }

        return result;
    }
}