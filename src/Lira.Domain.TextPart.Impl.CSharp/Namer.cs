using Lira.Common;
using Lira.Domain.TextPart.Impl.CSharp.DynamicModel;

namespace Lira.Domain.TextPart.Impl.CSharp;

record DynamicNames(string Class, string Assembly);

class Namer(Namer.State state)
{
    public class State
    {
        // cannot be stored in a static variable,
        // because in tests several FunctionFactory are created in parallel
        // and because of this caching does not work correctly
        public int RevisionCounter { get; set; }
    }

    public int Revision { get;  } = ++state.RevisionCounter;

    public DynamicNames GetNames(string prefix, IEnumerable<string> codes)
    {
        string className = GetClassName(prefix, codes);
        return new DynamicNames(className, GetAssemblyName(className));
    }

    public DynamicNames GetNames(string prefix, params ClassWithoutName[] codes) => GetNames(prefix, codes.Select(x => x.SetClassName("UnknownYet")));

    public readonly string AssemblyPrefix = "__dynamic";
    private string GetAssemblyName(string name) => $"{AssemblyPrefix}_{Revision}_{name}";

    private string GetClassName(string prefix, IEnumerable<string> codes)
    {
        return prefix + "_" + Sha1.Create(string.Concat(codes));
    }
}