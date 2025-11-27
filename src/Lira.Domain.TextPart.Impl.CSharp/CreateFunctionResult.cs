namespace Lira.Domain.TextPart.Impl.CSharp;

public abstract record CreateFunctionResult<TFunction>
{
    public record Success(TFunction Function) : CreateFunctionResult<TFunction>;
    public record Failed(string Message) : CreateFunctionResult<TFunction>;
}