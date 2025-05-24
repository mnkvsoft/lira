namespace Lira.Domain.TextPart.Impl.Custom;

public abstract class DeclaredItem : IObjectTextPart, IEquatable<DeclaredItem>
{
    public abstract string Name { get; }
    public abstract IEnumerable<dynamic?> Get(RuleExecutingContext context);
    public abstract ReturnType? ReturnType { get; }

    public bool Equals(DeclaredItem? other)
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

        return Equals((DeclaredItem)obj);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}