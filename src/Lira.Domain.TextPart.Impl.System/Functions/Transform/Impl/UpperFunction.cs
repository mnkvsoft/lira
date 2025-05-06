namespace Lira.Domain.TextPart.Impl.System.Functions.Transform.Impl;

record UpperFunction : ITransformFunction
{
    public ReturnType ReturnType => ReturnType.String;

    public dynamic? Transform(dynamic? input)
    {
        object? value = input;
        return value?.ToString()?.ToUpper();
    }
}
