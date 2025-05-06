namespace Lira.Domain.TextPart;

public interface ITransformFunction
{
    dynamic? Transform(dynamic? dynamic);
    ReturnType ReturnType { get; }
}