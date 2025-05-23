// ReSharper disable UnusedMember.Global
// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

using Lira.Domain.TextPart.Impl.Custom;

namespace Lira.Domain.TextPart.Impl.CSharp.DynamicModel;

public class VariablesWriter(RuleExecutingContext context, IDeclaredItemsProvider declaredPartsProvider, bool readOnly)
{

    public dynamic this[string name]
    {
        get
        {
            throw new Exception("Read not supported");
        }
        set
        {
            if(readOnly)
                throw new InvalidOperationException("Cannot set property on a read-only object.");

            if (string.IsNullOrEmpty(name))
                throw new Exception("Name is empty");

            declaredPartsProvider.SetVariable(name, context, value);
        }
    }
}