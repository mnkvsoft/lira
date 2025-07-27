namespace Lira.Domain.Configuration.Parsing;

class OperatorDefinition
{
    public string Name { get; }
    public ParametersMode ParametersMode { get; }
    public IReadOnlyDictionary<string, AllowedChildElementDefinition> AllowedChildElements { get; }
    public bool WithBody { get; }

    public OperatorDefinition(string name, ParametersMode parametersMode, bool withBody = true, IReadOnlyDictionary<string, ParametersMode>? allowedChildElements = null)
    {
        WithBody = withBody;
        Name = name;
        ParametersMode = parametersMode;
        AllowedChildElements = allowedChildElements?.ToDictionary(x => x.Key, x => new AllowedChildElementDefinition(this, x.Key, x.Value)) ?? new Dictionary<string, AllowedChildElementDefinition>();
    }
}

record AllowedChildElementDefinition(OperatorDefinition Operator, string Name, ParametersMode ParametersMode);

enum ParametersMode
{
    Required,
    None,
    Maybe
}