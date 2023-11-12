using Lira.Common;

namespace Lira.Domain.TextPart.Impl.Custom.FunctionModel;

public class FunctionSet : UniqueSet<Function>
{
    public FunctionSet(IReadOnlyCollection<Function> functions) : base(functions)
    {
    }

    public FunctionSet()
    {
    }
}
