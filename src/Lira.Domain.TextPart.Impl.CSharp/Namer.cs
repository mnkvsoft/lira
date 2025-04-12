using Lira.Common;

namespace Lira.Domain.TextPart.Impl.CSharp;

class Namer
{
    public class State
    {
        // cannot be stored in a static variable,
        // because in tests several FunctionFactory are created in parallel
        // and because of this caching does not work correctly
        public int RevisionCounter { get; set; }
    }

    public int Revision { get;  }

    public Namer(State state)
    {
        Revision = ++state.RevisionCounter;
    }

    public readonly string AssemblyPrefix = "__dynamic";
    public string GetAssemblyName(string name) => $"{AssemblyPrefix}_{Revision}_{name}";

    public string GetClassName(string prefix, CodeBlock code) => GetClassName(prefix, code.ToString());

    public string GetClassName(string prefix, params string[] codes)
    {
        return prefix + "_" + Sha1.Create(string.Concat(codes));
    }
}