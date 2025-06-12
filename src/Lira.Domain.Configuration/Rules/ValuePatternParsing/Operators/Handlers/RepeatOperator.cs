using System.Text;
using Lira.Domain.TextPart;
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Handlers;

class RepeatHandler : IOperatorHandler
{
    public string OperatorName => OperatorPart.Prefix + "repeat";
    public IObjectTextPart CreateOperatorPart(OperatorDraft draft) => new RepeatOperator(draft);
}

class RepeatOperator(OperatorDraft draft) : IObjectTextPart
{
    private readonly IReadOnlyCollection<IObjectTextPart> _body = OperatorParser.Parse(draft).Body;
    public async IAsyncEnumerable<dynamic?> Get(RuleExecutingContext context)
    {
        int count = Random.Shared.Next(1, 5);

        var sb = new StringBuilder();
        for (int i = 0; i < count; i++)
        {
            if(sb.Length > 0)
                yield return ',';

            foreach (var part in _body)
            {
                yield return part;
            }
        }
    }

    public ReturnType ReturnType => ReturnType.String;
}