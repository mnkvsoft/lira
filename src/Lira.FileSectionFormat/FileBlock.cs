using Lira.Common.Extensions;

namespace Lira.FileSectionFormat;
public record FileBlock(string Name, IReadOnlyList<string> Lines) : IComparable<FileBlock>
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

internal class FileBlockBuilder
{
    private readonly List<string> _lines = new();
    private readonly string _name;

    public FileBlockBuilder(string name)
    {
        _name = name;
    }

    public void Add(string line) => _lines.Add(line);

    public FileBlock Build() => new(_name, _lines.TrimEmptyLines().ToArray());
}
