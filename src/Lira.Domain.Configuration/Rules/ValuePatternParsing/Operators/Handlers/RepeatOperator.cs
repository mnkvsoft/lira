using Lira.Common;
using Lira.Common.Extensions;
using Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Parsing;
using Lira.Domain.TextPart;
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Handlers;

class RepeatOperatorDefinition()
    : OperatorDefinition("repeat", ParametersMode.Maybe, withBody: true, allowedChildElements: null);

class RepeatHandler(TextPartsParserInternal parser, RepeatOperatorDefinition definition) : IOperatorHandler
{
    public OperatorDefinition Definition => definition;

    public async Task<OperatorPart> CreateOperatorPart(Token.Operator @operator, IParsingContext context,
        OperatorPartFactory operatorPartFactory)
    {
        Func<int> getCount;
        if (@operator.Parameters != null)
        {
            var pars = @operator.Parameters;
            if(pars.Type != OperatorParametersType.SingleLine)
                throw new Exception("The @repeat operator support only single line parameters (started with ':') yet");

            var value = pars.Value.Trim();
            if (Interval<int>.TryParse(value, out var interval))
            {
                if (interval.From < 2)
                    throw new Exception("The @repeat operator range argument must be start from 2. Current: " + value);

                getCount = () => Random.Shared.Next(interval);
            }
            else if (int.TryParse(value, out int count))
            {
                if (count < 2)
                    throw new Exception("The @repeat operator argument must be more or equals 2. Current: " + value);

                getCount = () => count;
            }
            else
            {
                throw new Exception("The @repeat operator expected integer argument or range. Current: " + value);
            }
        }
        else
        {
            getCount = () => Random.Shared.Next(3, 5);
        }

        return new RepeatOperator(await parser.Parse(@operator.Content, context, operatorPartFactory), getCount);
    }

    class RepeatOperator(IReadOnlyCollection<IObjectTextPart> body, Func<int> getCount) : OperatorPart
    {
        public override IEnumerable<dynamic?> Get(RuleExecutingContext context)
        {
            var count = getCount();
            for (int i = 0; i < count; i++)
            {
                if(i > 0)
                    // todo: use either the value passed by the user or try to calculate it based on the Content-Type header
                    yield return ",\n";

                foreach (var obj in body.GetAllObjects(context))
                {
                    yield return obj;
                }
            }
        }
    }
}

