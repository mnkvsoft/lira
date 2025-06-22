namespace Lira.Domain.Configuration.Parsing;

public enum ParametersMode
{
    Required,
    None,
    Maybe
}
public class OperatorDefinition
{
    public OperatorDefinition(string name, ParametersMode parametersMode, bool withBody = true, IReadOnlyDictionary<string, ParametersMode>? allowedChildElements = null)
    {
        WithBody = withBody;
        Name = name;
        AllowedChildElements = allowedChildElements?.ToDictionary(x => x.Key, x => new AllowedChildElementDefinition(this, x.Key, x.Value)) ?? new Dictionary<string, AllowedChildElementDefinition>();
    }

    public string Name { get; }
    public IReadOnlyDictionary<string, AllowedChildElementDefinition> AllowedChildElements { get; }
    public bool WithBody { get; }
}

public record AllowedChildElementDefinition(OperatorDefinition Operator, string Name, ParametersMode ParametersMode);