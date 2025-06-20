namespace Lira.Domain.Configuration.Parsing;

public class OperatorDefinition
{
    public OperatorDefinition(bool withBody, string name, IReadOnlyDictionary<string, (string Name, bool ParametersIsRequired)> allowedChildElements)
    {
        WithBody = withBody;
        Name = name;
        AllowedChildElements = allowedChildElements.ToDictionary(x => x.Key, x => new AllowedChildElementDefinition(this, x.Value.Name, x.Value.ParametersIsRequired));
    }

    public string Name { get; }
    public IReadOnlyDictionary<string, AllowedChildElementDefinition> AllowedChildElements { get; }
    public bool WithBody { get; }
}

public record AllowedChildElementDefinition(OperatorDefinition Operator, string Name, bool ParametersIsRequired);