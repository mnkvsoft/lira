using Lira.Common.Extensions;

namespace Lira.FileSectionFormat;
public record FileBlock(string Name, string? Key, IReadOnlyList<string> Lines) : IComparable<FileBlock>
{
    public int CompareTo(FileBlock? other)
    {
        if (ReferenceEquals(this, other))
        {
            return 0;
        }

        if (other is null)
        {
            return 1;
        }

        return string.Compare(Name, other.Name, StringComparison.Ordinal);
    }
}

internal class FileBlockBuilder(string name, string? key)
{
    private readonly List<string> _lines = new();

    public void Add(string line) => _lines.Add(line);

    public FileBlock Build() => new(name, key, _lines.TrimEmptyLines().ToArray());
}
