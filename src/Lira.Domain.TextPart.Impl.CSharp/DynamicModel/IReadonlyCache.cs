// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
namespace Lira.Domain.TextPart.Impl.CSharp.DynamicModel;

public interface IReadonlyCache
{
    bool contains(string key);
    dynamic get(string key);
}