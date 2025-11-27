using Lira.Common.Extensions;

namespace Lira.Domain.TextPart;

public interface ICustomDictsProvider
{
    CustomDic GetCustomDic(string name);
}

public class CustomDic(IReadOnlyList<string> list)
{
    private readonly HashSet<string> _hashSet = new(list);

    public string NextValue() => list.Random();

    public bool ValueIsBelong(string value) => _hashSet.Contains(value);
}