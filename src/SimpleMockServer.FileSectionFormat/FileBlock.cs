namespace SimpleMockServer.FileSectionFormat;
public class FileBlock
{
    public List<string> Lines = new List<string>();
    public string Name { get; }

    public FileBlock(string name)
    {
        Name = name;
    }
}