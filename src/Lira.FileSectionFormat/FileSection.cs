using System.Collections.Immutable;

namespace Lira.FileSectionFormat;

public record FileSection(
    string Name,
    string? Key,
    IImmutableList<string> LinesWithoutBlock,
    IImmutableList<FileSection> ChildSections,
    IReadOnlySet<FileBlock> Blocks);


public class FileSectionBuilder
{
    public string Name { get; }
    public string? Key { get; }

    public List<string> LinesWithoutBlock { get; } = new();
    public HashSet<FileBlock> Blocks { get; } = new();
    public List<FileSectionBuilder> ChildSections { get; } = new();

    public FileSectionBuilder(string name, string? key)
    {
        Name = name;
        Key = key;
    }

    public FileSection Build()
    {
        return new FileSection(
            Name,
            Key,
            LinesWithoutBlock.ToImmutableList(),
            ChildSections.Select(x => x.Build()).ToImmutableList(),
            Blocks);
    }
}