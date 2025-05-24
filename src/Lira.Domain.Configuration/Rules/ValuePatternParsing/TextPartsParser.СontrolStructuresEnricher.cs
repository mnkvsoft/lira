using Lira.Domain.TextPart;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing;

partial class TextPartsParser
{
    class OperatorPart(OperatorPart.Draft draft) : IObjectTextPart
    {
        public record Draft(string Name, string Parameters, List<IObjectTextPart> Content);

        public IObjectTextPart Value { get; set; }

        public Task<dynamic?> Get(RuleExecutingContext context) => Value.Get(context);

        public ReturnType? ReturnType => Value.ReturnType;
    }

    static class СontrolStructuresEnricher
    {
        public static IReadOnlyCollection<IObjectTextPart> AddOperators(
            List<IObjectTextPart> parts)
        {
            var root = new List<IObjectTextPart>();
            var currents = new Stack<List<IObjectTextPart>>();
            currents.Push(root);

            var ops = new List<OperatorPart>();
            var currentOperators = new Stack<OperatorPart>();

            var operators = new[] { "@repeat", "@random", "@end" };


            foreach (var part in parts)
            {
                if (part is not Static @static)
                {
                    currents.Peek().Add(part);
                }
                else
                {
                    var value = @static.Value;
                    var positions = GetPositions(value, operators);

                    if (positions.Count == 0)
                    {
                        currents.Peek().Add(part);
                    }
                    else
                    {
                        string beforePosition = value[..positions.First().Index];
                        if (beforePosition != string.Empty)
                        {
                            currents.Peek().Add(new Static(beforePosition));
                        }

                        for (int i = 0; i < positions.Count; i++)
                        {
                            var position = positions[i];
                            if (position.Name == "@@@")
                            {
                                if (currentOperators.Count == 0)
                                    throw new Exception("Missing begin operator for @@@");

                                currents.Pop();
                                currentOperators.Pop();
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

                                var content = new List<IObjectTextPart> { new Static(afterParameters) };
                                var @operator = new OperatorPart(new OperatorPart.Draft(position.Name, parameters, content));
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
            }

            return root;
        }

        static List<(string Name, int Index)> GetPositions(string str, string[] names)
        {
            string current = str;
            var result = new List<(string Name, int Index)>();
            int deletedLength = 0;

            while (current.Length > 0)
            {
                int nearIndex = current.Length - 1;
                string? nearName = null;
                foreach (var name in names)
                {
                    var idx = current.IndexOf(name, StringComparison.OrdinalIgnoreCase);
                    if (idx > 0 && idx < nearIndex)
                    {
                        nearIndex = idx;
                        nearName = name;
                    }
                }

                if (nearName != null)
                {
                    result.Add((nearName, nearIndex + deletedLength));
                    var newCurrent = current.Substring(nearIndex + nearName.Length);
                    deletedLength += (current.Length - newCurrent.Length);
                    current = newCurrent;
                }
                else
                {
                    return result;
                }
            }

            return result;
        }
    }
}