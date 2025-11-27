using Lira.Common;
using Lira.Common.Exceptions;
using Lira.Common.Extensions;
using Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Handlers.ParametersParsing;
using Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Parsing;
using Lira.Domain.TextPart;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Handlers;

class RepeatOperatorDefinition()
    : OperatorDefinition(
        "repeat",
        ParametersMode.Maybe,
        withBody: true,
        allowedChildElements: null);

class RepeatHandler(TextPartsParserInternal parser, RepeatOperatorDefinition definition) : IOperatorHandler
{
    const string DefaultSeparator = ",";
    private static readonly Func<int> DefaultGetCount = () => Random.Shared.Next(3, 5);

    public OperatorDefinition Definition => definition;

    class RepeatOperator(IReadOnlyCollection<IObjectTextPart> body, Func<int> getCount, string separator) : OperatorPart
    {
        public override IEnumerable<dynamic?> Get(RuleExecutingContext context)
        {
            var count = getCount();
            for (int i = 0; i < count; i++)
            {
                if (i > 0)
                    // todo: use either the value passed by the user or try to calculate it based on the Content-Type header
                    yield return separator;

                foreach (var obj in body.GetAllObjects(context))
                {
                    yield return obj;
                }
            }
        }
    }

    static class Parameters
    {
        public static readonly MethodParameterDefinition Count =
            MethodParameterDefinition.Int("count", isRequired: false);

        public static readonly MethodParameterDefinition Min =
            MethodParameterDefinition.Int("min", isRequired: false);

        public static readonly MethodParameterDefinition Max =
            MethodParameterDefinition.Int("max", isRequired: false);

        public static readonly MethodParameterDefinition Separator =
            MethodParameterDefinition.Str("separator", isRequired: false);

        public static readonly HashSet<MethodParameterDefinition> All = [Count, Min, Max, Separator];
    }

    public async Task<OperatorPart> CreateOperatorPart(Token.Operator @operator, IParsingContext context,
        OperatorPartFactory operatorPartFactory)
    {
        var getCount = DefaultGetCount;
        var separator = DefaultSeparator;

        if (@operator.Parameters != null)
        {
            var pars = @operator.Parameters;
            if (pars.Type == OperatorParametersType.SingleLine)
            {
                (getCount, separator) = GetCountFromSingleLineParams(pars);
            }
            else if (pars.Type == OperatorParametersType.Full)
            {
                (getCount, separator) = GetCountFromFullParams(pars);
            }
            else
            {
                throw new UnsupportedEnumValue(pars.Type);
            }
        }

        return new RepeatOperator(await parser.Parse(@operator.Content, context, operatorPartFactory), getCount, separator);
    }

    private static (Func<int> GetCount, string Separator) GetCountFromFullParams(OperatorParameters pars)
    {
        var paramsParseResult = FullParamsParser.Parse(Parameters.All, pars.Value, Parameters.Count);

        if (paramsParseResult is ParamsParseResult.Fail fail)
            throw new Exception("The @repeat operator has an error in defining parameters: " + fail.Message);

        var prs = (ParamsParseResult.Success)paramsParseResult;

        var countPar =
            (MethodParameter.Int?)prs.Parameters.SingleOrDefault(x => x.Definition == Parameters.Count);
        var minPar = (MethodParameter.Int?)prs.Parameters.SingleOrDefault(x => x.Definition == Parameters.Min);
        var maxPar = (MethodParameter.Int?)prs.Parameters.SingleOrDefault(x => x.Definition == Parameters.Max);
        var separatorPar =
            (MethodParameter.String?)prs.Parameters.SingleOrDefault(x => x.Definition == Parameters.Separator);

        string separator = separatorPar == null ? DefaultSeparator : separatorPar.Value;

        if (countPar != null)
        {
            if (minPar != null)
                throw new Exception(
                    "The @repeat operator has an error in defining parameters: 'min' parameter cannot be defined if 'count' is already defined");

            if (maxPar != null)
                throw new Exception(
                    "The @repeat operator has an error in defining parameters: 'max' parameter cannot be defined if 'count' is already defined");

            return (() => countPar.Value, separator);
        }

        if (minPar != null)
        {
            int min = minPar.Value;
            if (maxPar != null)
            {
                int max = maxPar.Value;
                if(max < min)
                    throw new Exception(
                        "The @repeat operator has an error in defining parameters: 'max' parameter cannot be less than 'min' parameter'");

                return (() => Random.Shared.Next(min, max + 1), separator);
            }

            return (() => Random.Shared.Next(min, min * 2), separator);
        }

        if (maxPar != null)
        {
            int max = maxPar.Value;
            if(max < 2)
                throw new Exception(
                    "The @repeat operator has an error in defining parameters: 'max' parameter cannot be less than 2");

            return (() => Random.Shared.Next(1, maxPar.Value + 1), separator);
        }

        return (() => Random.Shared.Next(3, 5), separator);
    }

    private static (Func<int>, string Separator) GetCountFromSingleLineParams(OperatorParameters pars)
    {
        var value = pars.Value.Trim();
        if (Interval<int>.TryParse(value, out var interval))
        {
            if (interval.From < 2)
                throw new Exception("The @repeat operator range argument must be start from 2. Current: " +
                                    value);

            return (() => Random.Shared.Next(interval), DefaultSeparator);
        }

        if (int.TryParse(value, out int count))
        {
            if (count < 2)
                throw new Exception("The @repeat operator argument must be more or equals 2. Current: " +
                                    value);

            return (() => count, DefaultSeparator);
        }

        throw new Exception("The @repeat operator expected integer argument or range. Current: " + value);
    }


}