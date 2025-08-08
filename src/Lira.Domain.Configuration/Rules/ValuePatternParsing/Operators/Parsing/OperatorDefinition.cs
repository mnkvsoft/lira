namespace Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Parsing;

class OperatorDefinition
{
    public string Name { get; }
    public ParametersMode ParametersMode { get; }
    public IReadOnlyDictionary<string, AllowedChildElementDefinition> AllowedChildElements { get; }
    public bool WithBody { get; }

    public OperatorDefinition(string name, ParametersMode parametersMode, bool withBody, IReadOnlyDictionary<string, ParametersMode>? allowedChildElements)
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