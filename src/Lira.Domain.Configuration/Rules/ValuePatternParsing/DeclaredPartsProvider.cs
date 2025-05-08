using Lira.Domain.Configuration.Extensions;
using Lira.Domain.TextPart;
using Lira.Domain.TextPart.Impl.CSharp;
using Lira.Common.Extensions;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing;

class DeclaredPartsProvider(IReadonlyDeclaredItems items) : IDeclaredPartsProvider
{
    public IObjectTextPart Get(string name) => items.Get(name);

    public ReturnType? GetPartType(string name)
    {
        var variable = items.Variables
            .FirstOrDefault(v => v.Name == name.TrimStart(Consts.ControlChars.RuleVariablePrefix));

        if (variable != null)
            return variable.ReturnType;

        var function = items.Functions
            .FirstOrDefault(v => v.Name == name.TrimStart(Consts.ControlChars.FunctionPrefix));

        return function?.ReturnType;
    }

    public void SetVariable(string name, RuleExecutingContext context, dynamic value)
    {
        var variable = items.Variables
            .Single(v => v.Name == name.TrimStart(Consts.ControlChars.RuleVariablePrefix));

        variable.SetValue(context, value);
    }
}