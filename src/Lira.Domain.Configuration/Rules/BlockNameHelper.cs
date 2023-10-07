using System.Reflection;
using Lira.Common.Extensions;

namespace Lira.Domain.Configuration.Rules;

public class BlockNameHelper
{

    public static IReadOnlySet<string> GetBlockNames<T>()
    {
        var set = new HashSet<string>();
        var values = GetAllPublicConstantValues<string>(typeof(T));

        foreach (var value in values)
        {
            if (string.IsNullOrEmpty(value))
                throw new Exception("Empty block name");

            set.AddOrThrowIfContains(value);
        }

        return set;
    }

    private static IReadOnlyCollection<T?> GetAllPublicConstantValues<T>(Type type)
    {
        return type
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(T))
            .Select(x => (T?)x.GetRawConstantValue())
            .ToList();
    }
}
