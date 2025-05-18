using Lira.Domain.TextPart;

namespace Lira.Domain.Configuration.DeclarationItems;

class DeclaredItemDraft : IEquatable<DeclaredItemDraft>
{
    public string Name { get; }
    public string Pattern { get; }
    public ReturnType? ReturnType { get; }
    public string Source { get; }

    public DeclaredItemDraft(string name, string pattern, ReturnType? returnType, string source)
    {
        Name = name;
        Pattern = pattern;
        ReturnType = returnType;
        Source = source;
    }

    public bool Equals(DeclaredItemDraft? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Name == other.Name;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((DeclaredItemDraft)obj);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}