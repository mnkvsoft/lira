using ArgValidation;

namespace Lira.Domain.Configuration.Templating;

public class Template : IEquatable<Template>
{
    public string Name { get; }
    public string Value { get; }

    public Template(string name, string value)
    {
        Arg.NotNullOrEmpty(name, nameof(name)); 
        Arg.NotNullOrEmpty(value, nameof(value)); 

        Name = name;
        Value = value;
    }

    public bool Equals(Template? other)
    {
        if (ReferenceEquals(null, other))
            return false; 

        return Name == other.Name;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Template);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}
