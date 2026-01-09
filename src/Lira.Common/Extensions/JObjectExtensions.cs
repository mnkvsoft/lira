using System.Dynamic;
using Newtonsoft.Json.Linq;

namespace Lira.Common.Extensions;

public static class JObjectExtensions
{
    public static object? GetFieldValue(this JObject jObject, GetMemberBinder binder)
    {
        var name = binder.Name;
        if (string.IsNullOrEmpty(name))
            throw new Exception("Name is empty");

        var token = jObject.GetValue(name);

        if (token == null)
            throw new Exception($"Field '{name}' not found");

        if (token is JValue value)
            return value.Value;

        if (token is JObject obj)
            return obj;

        if (token is JArray array)
            return array;

        throw new Exception($"Unknown token type for '{name}': " + token);
    }
}