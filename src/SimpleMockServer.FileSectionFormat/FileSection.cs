namespace SimpleMockServer.FileSectionFormat;



public class FileSection
{
    public string Name { get; }
    public List<string> LinesWithoutBlock = new List<string>();
    public HashSet<FileBlock> Blocks = new HashSet<FileBlock>();
    public List<FileSection> ChildSections = new List<FileSection>();

    public FileSection(string name)
    {
        Name = name;
    }
}