namespace Lira.Domain.TextPart.Impl.System.Functions.Transform.Impl.Format;

record FormatFunction(string Format) : ITransformFunction
{
    public ReturnType ReturnType => ReturnType.String;

    public object? Transform(object? input)
    {
        return input?.FormatOrThrow(Format);
    }
}
