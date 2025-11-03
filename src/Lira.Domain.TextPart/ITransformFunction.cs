namespace Lira.Domain.TextPart;

public interface ITransformFunction
{
    dynamic? Transform(dynamic? dynamic);
    Type Type { get; }
}