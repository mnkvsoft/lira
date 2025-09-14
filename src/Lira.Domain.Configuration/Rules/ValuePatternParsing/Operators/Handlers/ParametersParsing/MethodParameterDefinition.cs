using System.Globalization;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Handlers.ParametersParsing;

class MethodParameterDefinition : IEquatable<MethodParameterDefinition>
{
    public string Name { get; }
    public bool IsRequired { get; }
    public Type Type { get; }

    private MethodParameterDefinition(string name, bool isRequired, Type type)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        Name = name;
        IsRequired = isRequired;
        Type = type;
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    public virtual bool Equals(MethodParameterDefinition? other)
    {
        if(other == null)
            return false;

        return other.Name == Name;
    }

    public static MethodParameterDefinition Bool(string name, bool isRequired) => new(name, isRequired, typeof(MethodParameter.Bool));
    public static MethodParameterDefinition Str(string name, bool isRequired) => new(name, isRequired, typeof(MethodParameter.String));
    public static MethodParameterDefinition Int(string name, bool isRequired) => new(name, isRequired, typeof(MethodParameter.Int));
    public static MethodParameterDefinition Dec(string name, bool isRequired) => new(name, isRequired, typeof(MethodParameter.Dec));
}

internal record MethodParameter(MethodParameterDefinition Definition)
{
    public record String(MethodParameterDefinition Definition, string Value) : MethodParameter(Definition)
    {
        public override string ToString()
        {
            return $"[{Definition.Name}:str = '{Value}']";
        }
    }

    public record Bool(MethodParameterDefinition Definition, bool Value) : MethodParameter(Definition)
    {
        public override string ToString()
        {
            return $"[{Definition.Name}:bool = {Value.ToString().ToLower()}]";
        }
    }

    public record Int(MethodParameterDefinition Definition, int Value) : MethodParameter(Definition)
    {
        public override string ToString()
        {
            return $"[{Definition.Name}:int = {Value}]";
        }
    }

    public record Dec(MethodParameterDefinition Definition, double Value) : MethodParameter(Definition)
    {
        public override string ToString()
        {
            return $"[{Definition.Name}:dec = {Value.ToString(CultureInfo.InvariantCulture)}]";
        }
    }
}