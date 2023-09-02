using ArgValidation;
using SimpleMockServer.Common;

namespace SimpleMockServer.Domain.TextPart.Variables;

public abstract class Variable : IObjectTextPart, IEquatable<Variable>, IUniqueSetItem
{
    public string Name { get; }
    public string EntityName => "Variable";

    protected Variable(string name)
    {
        Arg.NotNullOrEmpty(name, nameof(name));

        // restriction need for replace variable in csharp block
        if (!IsValidName(name))
            throw new ArgumentException($"Variable name must contains only letters or _. Current: '{name}'");
        
        Name = name;
    }

    public static bool IsValidName(string value)
    {
        foreach (char c in value)
        {
            if (IsAllowedCharInName(c))
                continue;
            return false;
        }

        return true;
    }

    public static bool IsAllowedCharInName(char c) => char.IsLetter(c) || c == '_' || c == '.';

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

    public abstract dynamic? Get(RequestData request);
}
