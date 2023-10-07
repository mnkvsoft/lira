namespace Lira.Domain.Configuration;

public class FileParsingException : Exception
{
    public string FilePath { get; }

    public FileParsingException(string filePath, Exception innerException) : base("An error occured while parsing file: " + filePath, innerException)
    {
        FilePath = filePath;
    }
}
