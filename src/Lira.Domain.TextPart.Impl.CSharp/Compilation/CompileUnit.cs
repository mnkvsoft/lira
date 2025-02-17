using System.Collections.Immutable;

namespace Lira.Domain.TextPart.Impl.CSharp.Compilation;

record CompileUnit(
    string AssemblyName,
    IImmutableList<string> Codes,
    References References);