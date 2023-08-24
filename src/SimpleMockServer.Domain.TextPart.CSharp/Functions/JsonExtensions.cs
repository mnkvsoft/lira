using Newtonsoft.Json.Linq;

namespace SimpleMockServer.Domain.TextPart.CSharp.Functions;

public static class JsonUtils
{
    public record Json(JToken Value)
    {
        public Json replace(string path, object newValue)
        {
            return new Json(Value.ReplacePath(path, newValue));
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public static Json json(string json)
    {
        return new Json(JToken.Parse(json));
    }
    
    public static string replace(this string json, string path, object newValue)
    {
        return JToken.Parse(json).ReplacePath(path, newValue).ToString();
    }

    private static JToken ReplacePath(this JToken root, string path, object newValue)
    {
        if (root == null || path == null)
            throw new ArgumentNullException();

        foreach (var value in root.SelectTokens(path).ToList())
        {
            if (value == root)
                root = JToken.FromObject(newValue);
            else
                value.Replace(JToken.FromObject(newValue));
        }

        return root;
    }
}