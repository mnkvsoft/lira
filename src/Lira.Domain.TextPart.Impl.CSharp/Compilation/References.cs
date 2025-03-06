using System.Collections.Immutable;

namespace Lira.Domain.TextPart.Impl.CSharp.Compilation;

record References(
    IReadOnlyCollection<PeImage> Runtime,
    IImmutableList<string> AssembliesLocations);