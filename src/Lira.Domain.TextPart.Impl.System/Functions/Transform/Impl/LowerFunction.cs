namespace Lira.Domain.TextPart.Impl.System.Functions.Transform.Impl;

record LowerFunction : ITransformFunction
{
    public ReturnType ReturnType => ReturnType.String;

    public dynamic? Transform(dynamic? input)
    {
        object? value = input;
        return value?.ToString()?.ToLower();
    }
}
