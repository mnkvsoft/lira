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

        for (int i = 0; i < count; i++)
        {
            if(i > 0)
                yield return ',';

            await foreach (var obj in _body.GetAllObjects(context))
            {
                yield return obj;
            }
        }
    }

    public ReturnType ReturnType => ReturnType.String;
}