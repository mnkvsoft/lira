using Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Parsing;
using Lira.Domain.TextPart;
using Lira.Domain.TextPart.Impl.CSharp;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Handlers;

class IfOperatorDefinition() : OperatorDefinition(
    "if",
    ParametersMode.Required,
    withBody: true,
    allowedChildElements: new Dictionary<string, ParametersMode>
    {
        { "else", ParametersMode.Maybe },
        { "else if", ParametersMode.Required }
    });

class IfHandler(TextPartsParserInternal parser, IfOperatorDefinition operatorDefinition, IFunctionFactoryCSharpFactory functionFactoryCSharpFactory) : IOperatorHandler
{
    public OperatorDefinition Definition => operatorDefinition;

    public async Task<OperatorPart> CreateOperatorPart(Token.Operator @operator, IParsingContext context, OperatorPartFactory operatorPartFactory)
    {
        var elements = @operator.Elements;

        IReadOnlyCollection<IObjectTextPart>? elseParts = null;
        if (elements.Count > 0)
        {
            var elseCount = elements.Count(x => x.Definition.Name == "else");
            if (elseCount != 0)
            {
                if (elseCount != 1)
                    throw new Exception("The @if operator must contains only one 'else' block");

                var last = elements.Last();
                if(last.Definition.Name != "else")
                    throw new Exception("The @if operator can contain element 'else' only as the last one");

                elseParts = await parser.Parse(last.Content, context, operatorPartFactory);
            }
        }

        var predicates = new List<(IPredicateFunction predicate, IReadOnlyCollection<IObjectTextPart> parts)>(elements.Count + 1)
        {
            await GetPredicateAndParts(
                @operator.Parameters?.Value ?? throw new Exception("Empty parameters"),
                @operator.Content,
                context,
                operatorPartFactory)
        };

        for (int i = 0; i < elements.Count - (elseParts == null ? 0 : 1); i++)
        {
            var elseIfElem = elements[i];

            predicates.Add(await GetPredicateAndParts(
                elseIfElem.Parameters?.Value ?? throw new Exception("Empty parameters"),
                elseIfElem.Content,
                context,
                operatorPartFactory));
        }

        return new IfOperator(predicates, elseParts);
    }

    private async Task<(IPredicateFunction, IReadOnlyCollection<IObjectTextPart>)> GetPredicateAndParts(
        string? predicateCode,
        List<Token> content,
        IParsingContext context,
        OperatorPartFactory operatorPartFactory)
    {
        var parts = await parser.Parse(content, context, operatorPartFactory);
        var predicate = await GetPredicate(predicateCode, context);
        return (predicate, parts);
    }

    private async Task<IPredicateFunction> GetPredicate(string code, IParsingContext context)
    {
        var functionFactoryCSharp = await functionFactoryCSharpFactory.Get();
        var functionFactoryRuleContext = new FunctionFactoryRuleContext(context.CSharpUsingContext, new DeclaredItemsProvider(context.DeclaredItems));
        var predicateFunctionResult = functionFactoryCSharp.TryCreatePredicateFunction(functionFactoryRuleContext, code);
        return predicateFunctionResult.GetFunctionOrThrow(code, context);
    }

    class IfOperator(IReadOnlyCollection<(IPredicateFunction predicate, IReadOnlyCollection<IObjectTextPart> parts)> conditions, IReadOnlyCollection<IObjectTextPart>? elseParts) : OperatorPart
    {
        public override IEnumerable<dynamic?> Get(RuleExecutingContext context)
        {
            foreach (var condition in conditions)
            {
                if (condition.predicate.IsMatch(context))
                {
                    foreach (var part in condition.parts)
                    {
                        foreach (var value in part.Get(context))
                        {
                            yield return value;
                        }
                    }
                    yield break;
                }
            }

            if (elseParts != null)
            {
                foreach (var part in elseParts)
                {
                    foreach (var value in part.Get(context))
                    {
                        yield return value;
                    }
                }
            }
        }
    }
}

