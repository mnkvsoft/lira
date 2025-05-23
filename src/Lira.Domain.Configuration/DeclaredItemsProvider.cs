using System.Diagnostics.CodeAnalysis;
using Lira.Domain.TextPart;
using Lira.Domain.TextPart.Impl.Custom;
using Lira.Domain.TextPart.Impl.Custom.FunctionModel;
using Lira.Domain.TextPart.Impl.Custom.VariableModel;
using Lira.Domain.TextPart.Impl.Custom.VariableModel.LocalVariables;
using Lira.Domain.TextPart.Impl.Custom.VariableModel.RuleVariables;

namespace Lira.Domain.Configuration;

class DeclaredItemsProvider(IReadOnlySet<DeclaredItem> items) : IDeclaredItemsProvider
{
    public IObjectTextPart Get(string name)
    {
        if(!TryGet(name, out var result))
            throw new Exception($"Unknown declaration '{name}'");
        return result;
    }

    public bool TryGet(string name, [MaybeNullWhen(false)] out IObjectTextPart result)
    {
        result = items.SingleOrDefault(x => x.Name == name);
        return result != null;
    }

    public bool ItsAccessToDeclaredItem(string name, [MaybeNullWhen(false)] out IObjectTextPart part)
    {
        part = null;
        if (RuleVariable.IsValidName(name))
        {
            part = Get(name);
            return true;
        }

        if (LocalVariable.IsValidName(name))
        {
            part = Get(name);
            return true;
        }

        if (Function.IsValidName(name))
        {
            part = Get(name);
            return true;
        }

        return false;
    }

    public void SetVariable(string name, RuleExecutingContext context, dynamic value)
    {
        var variable = items.OfType<Variable>().Single(v => v.Name == name);
        variable.SetValue(context, value);
    }
}