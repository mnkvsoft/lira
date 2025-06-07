using Lira.Common.Extensions;
using Lira.Domain.TextPart;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators;

class OperatorsEnricher(OperatorPartFactory operatorPartFactory)
{
    private readonly OperatorPartFactory _operatorPartFactory = operatorPartFactory;

    public IReadOnlyCollection<IObjectTextPart> Enrich(
        List<IObjectTextPart> parts)
    {
        var root = new List<IObjectTextPart>();
        var currents = new Stack<List<IObjectTextPart>>();
        currents.Push(root);

        var ops = new List<OperatorPart>();
        var currentOperators = new Stack<OperatorPart>();

        var end = OperatorPart.End;
        var operators = _operatorPartFactory.RegisteredOperators.Union([OperatorPart.End]).ToArray();

        foreach (var part in parts)
        {
            if (part is not StaticPart @static)
            {
                currents.Peek().Add(part);
            }
            else
            {
                var value = @static.Value;
                var positions = value.GetPositions(operators);

                if (positions.Count == 0)
                {
                    currents.Peek().Add(part);
                }
                else
                {
                    string beforePosition = value[..positions.First().Index];
                    if (beforePosition != string.Empty)
                    {
                        currents.Peek().Add(new StaticPart(beforePosition));
                    }

                    for (int i = 0; i < positions.Count; i++)
                    {
                        var position = positions[i];
                        if (position.Name == end)
                        {
                            if (currentOperators.Count == 0)
                                throw new Exception("Missing begin operator for @end");

                            currents.Pop();
                            currentOperators.Pop();

                            var isLast = i == positions.Count - 1;
                            var from = position.Index + position.Name.Length;
                            var to = (isLast ? value.Length : positions[i + 1].Index);
                            var betweenPositions = value[from..to];

                            currents.Peek().Add(new StaticPart(betweenPositions));
                        }
                        else
                        {
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

                            var content = new List<IObjectTextPart> { new StaticPart(afterParameters) };
                            var @operator = new OperatorPart(new OperatorDraft(position.Name, parameters, content));
                            ops.Add(@operator);
                            currents.Peek().Add(@operator);
                            currents.Push(content);
                            currentOperators.Push(@operator);
                        }
                    }
                }
            }
        }

        if (ops.Count > 0)
        {
            foreach (var op in ops)
            {
                op.Value = _operatorPartFactory.CreateOperatorPart(op.Draft);
            }
        }

        return root;
    }
}