namespace Lira.Domain.TextPart;

public class TypeInfo(Type sourceType, ExplicitType? castTo)
{
    public readonly static TypeInfo UnknownWithoutCast = new(DotNetType.Unknown, null);

    public Type TargetType { get; } = castTo?.DotnetType ?? (sourceType == DotNetType.EnumerableDynamic ? typeof(string) : sourceType);

    public bool TryGet(dynamic? value, out dynamic? result, out Exception? exc)
    {
        result = null;
        exc = null;

        if (castTo != null)
        {
            if (!TypedValueCreator.TryCreate(castTo, value, out dynamic valueTyped, out exc))
                return false;

            result = valueTyped;
            return true;
        }

        if (value == null)
        {
            result = null;
            return true;
        }

        var type = value.GetType();
        if (TargetType == DotNetType.String && value is IEnumerable<dynamic> enumerable)
        {
            result = enumerable.GenerateString();
            return true;
        }

        if (TargetType == DotNetType.Unknown)
        {
            result = value;
            return true;
        }

        if (type != TargetType)
        {
            throw new Exception($"Expected value of type '{TargetType}' but passed '{value}' of type '{type}'");
        }

        result = value;
        return true;
    }
}