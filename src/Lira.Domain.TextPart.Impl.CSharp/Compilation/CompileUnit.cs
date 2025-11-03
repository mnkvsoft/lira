using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Lira.Domain.TextPart.Impl.CSharp.Compilation;

record CompileUnit(
    string AssemblyName,
    IImmutableList<SyntaxTree> SyntaxTrees,
    References References);