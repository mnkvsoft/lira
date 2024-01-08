namespace Lira.Domain;

public record ResponseData(
    int? StatusCode, 
    IReadOnlyCollection<Header> Headers, 
    IReadOnlyCollection<string> BodyParts);
    
public record Header(string Name, string? Value);