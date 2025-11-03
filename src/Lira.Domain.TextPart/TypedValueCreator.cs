using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Lira.Domain.TextPart.Types;

namespace Lira.Domain.TextPart;

public static class TypedValueCreator
{
    public static bool TryCreate(ExplicitType type, dynamic? value, [MaybeNullWhen(false)] out dynamic result, out Exception? exception)
    {
        exception = null;
        if (type == ExplicitType.Json)
            return TryCreateJson(value, out result, out exception);

        if (type == ExplicitType.String)
            return TryCreateString(value, out result);

        if (type == ExplicitType.Int)
            return TryCreateInt(value, out result);

        if (type == ExplicitType.Date)
            return TryCreateDate(value, out result);

        if (type == ExplicitType.Guid)
            return TryCreateGuid(value, out result);

        if (type == ExplicitType.Decimal)
            return TryCreateDecimal(value, out result);

        if (type == ExplicitType.Bool)
            return TryCreateBool(value, out result);

        throw new ArgumentOutOfRangeException(nameof(type), type, null);
    }

    private static bool TryCreateString(dynamic? value, out dynamic? result)
    {
        result = null;

        if (value is string)
        {
            result= value;
            return true;
        }

        if (value is null)
            return true;

        if (value is IConvertible convertible)
        {
            result = convertible.ToString(CultureInfo.InvariantCulture);
            return true;
        }

        result = value.ToString();
        return true;
    }

    private static bool TryCreateInt(dynamic? value, [MaybeNullWhen(false)] out dynamic result)
    {
        result = null;

        if (value is null)
            return false;

        if (value is long)
        {
            result = value;
            return true;
        }

        if (value is IConvertible convertible)
        {
           result = convertible.ToInt64(CultureInfo.InvariantCulture);
           return true;
        }

        if (long.TryParse(value.ToString(), CultureInfo.InvariantCulture, out long longValue))
        {
            result = longValue;
            return true;
        }

        return false;
    }

    private static bool TryCreateDecimal(dynamic? value, [MaybeNullWhen(false)] out dynamic result)
    {
        result = null;

        if (value is null)
            return false;

        if (value is decimal)
        {
            result = value;
            return true;
        }

        if (value is IConvertible convertible)
        {
            result = convertible.ToDecimal(CultureInfo.InvariantCulture);
            return true;
        }

        if (decimal.TryParse(value.ToString(), CultureInfo.InvariantCulture, out decimal dec))
        {
            result = dec;
            return true;
        }

        return false;
    }

    private static bool TryCreateDate(dynamic? value, [MaybeNullWhen(false)] out dynamic result)
    {
        result = null;

        if (value is null)
            return false;

        if (value is DateTime)
        {
            result = value;
            return true;
        }

        if (value is IConvertible convertible)
        {
            result = convertible.ToDateTime(CultureInfo.InvariantCulture);
            return true;
        }

        if (DateTime.TryParse(value.ToString(), CultureInfo.InvariantCulture, out DateTime date))
        {
            result = date;
            return true;
        }

        return false;
    }

    private static bool TryCreateGuid(dynamic? value, [MaybeNullWhen(false)] out dynamic result)
    {
        result = null;

        if (value is null)
            return false;

        if (value is Guid)
        {
            result = value;
            return true;
        }

        if (Guid.TryParse(value.ToString(), CultureInfo.InvariantCulture, out Guid guid))
        {
            result = guid;
            return true;
        }

        return false;
    }

    private static bool TryCreateBool(dynamic? value, [MaybeNullWhen(false)] out dynamic result)
    {
        result = null;

        if (value is null)
            return false;

        if (value is bool)
        {
            result = value;
            return true;
        }

        if (bool.TryParse(value.ToString(), out bool b))
        {
            result = b;
            return true;
        }

        return false;
    }

    private static bool TryCreateJson(dynamic? value, [MaybeNullWhen(false)] out dynamic result, out Exception? exception)
    {
        result = null;
        exception = null;

        if (value is null)
            return false;

        if (value is Json)
        {
            result = value;
            return true;
        }

        if (value is not string json)
            return false;

        try
        {
            result = Json.Parse(json);
            return true;
        }
        catch (Exception e)
        {
            exception = e;
            return false;
        }
    }
}