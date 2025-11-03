namespace Lira.Domain.TextPart;

public static class DotNetType
{
    record UnknownType;
    record DynamicType;

    public static readonly Type EnumerableDynamic = typeof(IEnumerable<dynamic>);
    public static readonly Type String = typeof(string);
    public static readonly Type Dynamic = typeof(DynamicType);
    public static readonly Type Unknown = typeof(UnknownType);
    public static readonly Type Int64 = typeof(Int64);
    public static readonly Type Guid = typeof(Guid);
    public static readonly Type Double = typeof(double);
    public static readonly Type DateTime = typeof(DateTime);
    public static readonly Type Bool = typeof(bool);
}