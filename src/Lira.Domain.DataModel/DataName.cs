﻿using ArgValidation;

namespace Lira.Domain.DataModel;

public struct DataName : IEquatable<DataName>
{
    private readonly string _value;

    public DataName(string value)
    {
        Arg.Validate(value, nameof(value))
            .NotNullOrWhitespace();

        _value = value.ToLower();
    }

    public bool Equals(DataName other)
    {
        return _value == other._value;
    }

    public override bool Equals(object? obj)
    {
        return obj is DataName other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public override string ToString()
    {
        return _value;
    }
}
