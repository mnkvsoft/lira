namespace Lira.Domain.TextPart.Impl.CSharp;

class KeyWord : IKeyWordInDynamicBlock
{
    public const string Using = "using";
    public string Word => Using;
}