namespace Lira.Domain.TextPart.Impl.System.Functions.Transform.Impl;

record LowerFunction : ITransformFunction
{
    public dynamic? Transform(dynamic? input)
    {
        object? value = input;
        return value?.ToString()?.ToLower();
    }
}
