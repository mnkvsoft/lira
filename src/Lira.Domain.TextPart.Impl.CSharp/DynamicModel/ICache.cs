namespace Lira.Domain.TextPart.Impl.CSharp.DynamicModel;

public interface ICache : IReadonlyCache
{
    void remove(string key);
    void set(string key, object value, string? time = null);
    void setFlag(string key, string? time = null);
}