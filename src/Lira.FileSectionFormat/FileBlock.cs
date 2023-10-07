namespace Lira.FileSectionFormat;
public class FileBlock
{
    private readonly List<string> _lines = new();
    public IReadOnlyList<string> Lines => _lines;

    public string Name { get; }

    public FileBlock(string name)
    {
        Name = name;
    }

    public void Add(string line) => _lines.Add(line);
}
