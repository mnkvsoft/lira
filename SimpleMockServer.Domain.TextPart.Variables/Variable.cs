using ArgValidation;
using SimpleMockServer.Domain.Generating;

namespace SimpleMockServer.Domain.TextPart.Variables;

public abstract class Variable : ITextPart, IEquatable<Variable>
{
    public string Name { get; }

    public Variable(string name)
    {
        Arg.NotNullOrEmpty(name, nameof(name)); 

        Name = name;
    }

    public bool Equals(Variable? other)
    {
        if (ReferenceEquals(null, other))
            return false; 

        return Name == other.Name;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Variable);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    public abstract string? Get(RequestData request);
}
