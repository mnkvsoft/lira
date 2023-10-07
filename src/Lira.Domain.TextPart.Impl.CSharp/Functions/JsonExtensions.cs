using Newtonsoft.Json.Linq;

namespace Lira.Domain.TextPart.Impl.CSharp.Functions;

public static class JsonUtils
{
    public record Json(JObject Value)
    {
        public Json replace(string path, object newValue)
        {
            return new Json(Value.ReplacePath(path, newValue));
        }

        public Json add(string name, object value)
        {
            return new Json(Value.AddToRoot(name, value));
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public static Json json(string json)
    {
        return new Json(JObject.Parse(json));
    }

    public static string replace(this string json, string path, object newValue)
    {
        return JObject.Parse(json).ReplacePath(path, newValue).ToString();
    }

    private static JObject ReplacePath(this JObject root, string path, object newValue)
    {
        if (root == null || path == null)
            throw new ArgumentNullException();

        foreach (var value in root.SelectTokens(path))
        {
            if (value == root)
                root = JObject.FromObject(newValue);
            else
                value.Replace(JToken.FromObject(newValue));
        }

        return root;
    }

    private static JObject AddToRoot(this JObject root, string name, object value)
    {
        root.Add(name, JToken.FromObject(value));
        return root;
    }
}