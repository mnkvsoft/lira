// ReSharper disable RedundantExplicitArrayCreation

namespace Lira.Domain.TextPart.Impl.CSharp;

public interface IGeneratingCSharpFactory : IDisposable
{
    IObjectTextPart Create(
        IDeclaredPartsProvider declaredPartsProvider,
        string code);

    ITransformFunction CreateTransform(string code);
}
