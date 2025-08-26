using Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Parsing;
using Lira.Domain.TextPart;
using Lira.Domain.TextPart.Impl.CSharp;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

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


        var ifCode = @operator.Parameters?.Value ?? throw new Exception("Empty parameters");
        var functionFactoryCSharp = await functionFactoryCSharpFactory.Get();
        var ifPredicateResult = functionFactoryCSharp.TryCreatePredicateFunction(
            new FunctionFactoryRuleContext(context.CSharpUsingContext,
                new DeclaredItemsProvider(context.DeclaredItems)),
            ifCode);

        var ifParts = await parser.Parse(@operator.Content, context, operatorPartFactory);
        var ifPredicate = ifPredicateResult.GetFunctionOrThrow(ifCode, context);

        var predicates = new List<(IPredicateFunction predicate, IReadOnlyCollection<IObjectTextPart> parts)>(elements.Count + 1)
        {
            (ifPredicate, ifParts)
        };

        for (int i = 0; i <  elements.Count - (elseParts == null ? 1 : 2); i++)
        {
            var elseIfElem = elements[i];
            var elseIfPredicate = ifPredicateResult.GetFunctionOrThrow(elseIfElem.Parameters?.Value ?? throw new Exception("Empty parameters"), context);
            var ifElseParts = await parser.Parse(elseIfElem.Content, context, operatorPartFactory);
            predicates.Add((elseIfPredicate, ifElseParts));
        }

        return new IfOperator(predicates, elseParts);
    }

    class IfOperator(IReadOnlyCollection<(IPredicateFunction predicate, IReadOnlyCollection<IObjectTextPart> parts)> conditions, IReadOnlyCollection<IObjectTextPart>? elseParts) : OperatorPart
    {
        public override async IAsyncEnumerable<dynamic?> Get(RuleExecutingContext context)
        {
            foreach (var condition in conditions)
            {
                if (condition.predicate.IsMatch(context))
                {
                    foreach (var part in condition.parts)
                    {
                        await foreach (var value in part.Get(context))
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
                    await foreach (var value in part.Get(context))
                    {
                        yield return value;
                    }
                }
            }
        }
    }
}

