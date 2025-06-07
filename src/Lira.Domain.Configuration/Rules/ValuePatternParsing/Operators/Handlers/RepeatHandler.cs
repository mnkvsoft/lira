using System.Text;
using Lira.Domain.TextPart;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Handlers;

class RepeatHandler : IOperatorHandler
{
    public string OperatorName => OperatorPart.Prefix + "repeat";
    public IObjectTextPart CreateOperatorPart(OperatorDraft draft) => new RepeatOperator(draft);
}

class RepeatOperator(OperatorDraft draft) : IObjectTextPart
{
    private readonly IReadOnlyCollection<IObjectTextPart> _body = OperatorParser.Parse(draft).Body;
    public async Task<dynamic?> Get(RuleExecutingContext context)
    {
        int count = Random.Shared.Next(1, 5);

        var sb = new StringBuilder();
        for (int i = 0; i < count; i++)
        {
            if(sb.Length > 0)
                sb.Append(',');

            sb.Append(await _body.Generate(context));
        }

        return sb.ToString();
    }

    public ReturnType ReturnType => ReturnType.String;
}