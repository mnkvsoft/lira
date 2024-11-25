using System.Diagnostics.CodeAnalysis;
using Lira.Domain.TextPart.Types;

namespace Lira.Domain.TextPart;

public record ReturnType
{
    public readonly static ReturnType Json = new("json", typeof(Json));
    public readonly static ReturnType String = new("str", typeof(string));
    public readonly static ReturnType Int = new("int", typeof(Int32));
    public readonly static ReturnType Guid = new("guid", typeof(Guid));
    public readonly static ReturnType Decimal = new("dec", typeof(decimal));
    public readonly static ReturnType Date = new("date", typeof(DateTime));

    private readonly string _value;
    public readonly Type DotnetType;

    private ReturnType(string value, Type dotnetType)
    {
        _value = value;
        DotnetType = dotnetType;
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