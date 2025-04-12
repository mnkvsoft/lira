// ReSharper disable RedundantExplicitArrayCreation

using Lira.Domain.Handling.Actions;

namespace Lira.Domain.TextPart.Impl.CSharp;

public interface IFunctionFactoryCSharpFactory
{
    FunctionFactoryUsingContext CreateRulesUsingContext(IReadOnlyCollection<string> fileLines);
    Task<IFunctionFactoryCSharp> Get();
}

public interface IFunctionFactoryCSharp
{
    CreateFunctionResult<IAction> TryCreateAction(FunctionFactoryRuleContext ruleContext, CodeBlock code);
    CreateFunctionResult<IObjectTextPart> TryCreateGeneratingFunction(FunctionFactoryRuleContext ruleContext, CodeBlock code);
    CreateFunctionResult<IMatchFunctionTyped> TryCreateMatchFunction(FunctionFactoryRuleContext ruleContext, CodeBlock code);
    CreateFunctionResult<IRequestMatcher> TryCreateRequestMatcher(FunctionFactoryRuleContext ruleContext, CodeBlock code);
    CreateFunctionResult<ITransformFunction> TryCreateTransformFunction(CodeBlock code);
}