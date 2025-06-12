#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
namespace Lira.Common;

public static class AsyncEnumerable
{
    public static async IAsyncEnumerable<T> Return<T>(T elem)
    {
        yield return elem;
    }
}