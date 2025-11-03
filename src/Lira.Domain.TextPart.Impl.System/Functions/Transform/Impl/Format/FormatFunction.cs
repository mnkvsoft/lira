namespace Lira.Domain.TextPart.Impl.System.Functions.Transform.Impl.Format;

record FormatFunction(string Format) : ITransformFunction
{
    public Type Type => DotNetType.String;

    public object? Transform(object? input)
    {
        return input?.FormatOrThrow(Format);
    }
}
