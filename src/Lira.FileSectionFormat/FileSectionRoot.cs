using System.Collections.Immutable;

namespace Lira.FileSectionFormat;

public record FileSectionRoot(IImmutableList<string> Lines, IImmutableList<FileSection> Sections)
{
    public static readonly FileSectionRoot Empty = new(ImmutableList<string>.Empty, ImmutableList<FileSection>.Empty);
}