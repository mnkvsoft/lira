using System.Diagnostics.CodeAnalysis;

namespace Lira.Domain.TextPart;

public record ExplicitType
{
    public readonly static ExplicitType Json = new("json", DotNetType.Dynamic);
    public readonly static ExplicitType String = new("str", DotNetType.String);
    public readonly static ExplicitType Int = new("int", DotNetType.Int64);
    public readonly static ExplicitType Guid = new("guid", DotNetType.Guid);
    public readonly static ExplicitType Decimal = new("dec", DotNetType.Double);
    public readonly static ExplicitType Date = new("date", DotNetType.DateTime);
    public readonly static ExplicitType Bool = new("bool", DotNetType.Bool);


    private readonly string _value;
    public readonly Type DotnetType;

    private ExplicitType(string value, Type dotnetType)
    {
        _value = value;
        DotnetType = dotnetType;
    }

    public static ExplicitType Parse(string value)
    {
        if(!TryParse(value, out var type))
            throw new FormatException($"Unknown type: '{value}'");
        return type;
    }

    private static bool TryParse(string value, [MaybeNullWhen(false)]out ExplicitType type)
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