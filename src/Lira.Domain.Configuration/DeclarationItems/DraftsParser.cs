using Lira.Common.Extensions;
using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.Domain.TextPart;
using Lira.Domain.TextPart.Impl.Custom.FunctionModel;
using Lira.Domain.TextPart.Impl.Custom.VariableModel.RuleVariables;
using Lira.Domain.TextPart.Impl.Custom.VariableModel.RuleVariables.Impl;

namespace Lira.Domain.Configuration.DeclarationItems;

internal class DeclaredItemDraftsParser(ITextPartsParser textPartsParser)
{
    public async Task<DeclaredItems> Parse(IReadOnlySet<DeclaredItemDraft> drafts, ParsingContext parsingContext)
    {
        var all = DeclaredItems.WithoutLocalVariables(parsingContext.DeclaredItems);
        var newContext = new ParsingContext(parsingContext, declaredItems: all);

        var onlyNew = new DeclaredItems();

        foreach (var draft in drafts.OrderByDependencies())
        {
            try
            {
                var part = await textPartsParser.Parse(draft.Pattern, newContext);

                var name = draft.Name;

                var typeInfo = new TypeInfo(part.Type, draft.CastTo);
                if (name.StartsWith(RuleVariable.Prefix))
                {
                    var variable = new DeclaredRuleVariable(name, part, typeInfo);
                    all.AddOrThrowIfContains(variable);
                    onlyNew.AddOrThrowIfContains(variable);
                }
                else if (name.StartsWith(Function.Prefix))
                {
                    var function = new Function(name, part, typeInfo);
                    all.AddOrThrowIfContains(function);
                    onlyNew.AddOrThrowIfContains(function);
                }
                else
                {
                    throw new Exception($"Unknown declaration type: '{name}'");
                }
            }
            catch (Exception e)
            {
                throw new Exception($"An error while parse {draft.Name}", e);
            }
        }

        return onlyNew;
    }
}