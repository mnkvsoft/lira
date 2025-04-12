namespace Lira.FileSectionFormat;
public class FileBlock : IComparable<FileBlock>
{
    private readonly List<string> _lines = new();
    public IReadOnlyList<string> Lines => _lines;

    public string Name { get; }

    public FileBlock(string name)
    {
        Name = name;
    }

    public void Add(string line) => _lines.Add(line);

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
