namespace Lira.FileSectionFormat;

public class FileSection
{
    public string Name { get; }
    public string? Key { get; }

    public List<string> LinesWithoutBlock { get; } = new();
    public HashSet<FileBlock> Blocks { get; } = new();
    public List<FileSection> ChildSections { get; } = new();

    public FileSection(string name, string? key)
    {
        Name = name;
        Key = key;
    }
}
