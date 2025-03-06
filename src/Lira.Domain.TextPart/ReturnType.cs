using System.Diagnostics.CodeAnalysis;
using Lira.Domain.TextPart.Types;

namespace Lira.Domain.TextPart;

public record ReturnType
{
    public readonly static ReturnType Json = new("json", typeof(Json), needTyped: false);
    public readonly static ReturnType String = new("str", typeof(string), needTyped: true);
    public readonly static ReturnType Int = new("int", typeof(Int64), needTyped: true);
    public readonly static ReturnType Guid = new("guid", typeof(Guid), needTyped: true);
    public readonly static ReturnType Decimal = new("dec", typeof(double), needTyped: true);
    public readonly static ReturnType Date = new("date", typeof(DateTime), needTyped: true);
    public readonly static ReturnType Bool = new("bool", typeof(bool), needTyped: true);

    private readonly string _value;
    public readonly Type DotnetType;
    public readonly bool NeedTyped;

    private ReturnType(string value, Type dotnetType, bool needTyped)
    {
        _value = value;
        DotnetType = dotnetType;
        NeedTyped = needTyped;
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

        if (value == "bool")
        {
            type = Bool;
            return true;
        }

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