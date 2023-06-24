using ArgValidation;

namespace SimpleMockServer.Common;

public class Hash : IEquatable<Hash>
{
    private readonly byte[] _hash;

    public Hash(byte[] hash)
    {
        Arg.NotEmpty(hash, nameof(hash));        
        _hash = hash;
    }
    
    public static Hash Parse(string value) => new(HexConverter.ToBytes(value));

    public override string ToString() => GetStringValue();

    
    private string? _toString;
    private string GetStringValue() => _toString ??= HexConverter.ToHexString(_hash);

    public bool Equals(Hash? other)
    {
        if (ReferenceEquals(null, other))
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return _hash.SequenceEqual(other._hash);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj.GetType() != this.GetType())
            return false;

        return Equals((Hash)obj);
    }

    public override int GetHashCode() => GetStringValue().GetHashCode();
}
