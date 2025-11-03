using Lira.Domain.TextPart;

namespace Lira.Domain.Configuration.DeclarationItems;

class DeclaredItemDraft : IEquatable<DeclaredItemDraft>
{
    public string Name { get; }
    public string Pattern { get; }
    public ExplicitType? CastTo { get; }

    public DeclaredItemDraft(string name, string pattern, ExplicitType? castTo)
    {
        Name = name;
        Pattern = pattern;
        CastTo = castTo;
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