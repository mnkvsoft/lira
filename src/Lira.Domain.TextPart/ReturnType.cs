using System.Diagnostics.CodeAnalysis;

namespace Lira.Domain.TextPart;

public record ReturnType
{
    public readonly static ReturnType Json = new("json");
    public readonly static ReturnType String = new("str");
    public readonly static ReturnType Int = new("int");
    public readonly static ReturnType Guid = new("guid");
    public readonly static ReturnType Decimal = new("dec");
    public readonly static ReturnType Date = new("date");

    private readonly string _value;

    private ReturnType(string value)
    {
        _value = value;
    }

    public static ReturnType Parse(string value)
    {
        if(!TryParse(value, out var type))
            throw new FormatException($"Unknown type: '{value}'");
        return type;
    }

    public static bool TryParse(string value, [MaybeNullWhen(false)]out ReturnType type)
    {
        type = null;

        if (value == "json")
        {
            type = Json;
            return true;
        }

        if (value == "dec")
        {
            type = Decimal;
            return true;
        }

        if (value == "str")
        {
            type = String;
            return true;
        }

        if (value == "guid")
        {
            type = Guid;
            return true;
        }

        if (value == "int")
        {
            type = Int;
            return true;
        }

        if (value == "date")
        {
            type = Date;
            return true;
        }

        return false;
    }

    public override string ToString()
    {
        return _value;
    }
}