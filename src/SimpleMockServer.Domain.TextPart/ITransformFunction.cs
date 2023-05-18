namespace SimpleMockServer.Domain.TextPart
{
    public interface ITransformFunction
    {
        object? Transform(object? input);
    }
}
