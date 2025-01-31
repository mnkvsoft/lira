namespace Lira.Domain.TextPart.Impl.CSharp.Compilation;

abstract record CompileResult
{
    public record Success(PeImage PeImage) : CompileResult;
    public record Fault(string Message) : CompileResult;
}