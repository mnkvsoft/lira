using Lira.Common.Extensions;
using Newtonsoft.Json.Linq;

namespace Lira.Domain.TextPart.Types;

public class Json
{
    private JObject Value { get; }

    public Json(string value)
    {
        Value = JObject.Parse(value);
    }

    private Json(JObject value)
    {
        Value = value;
    }

    public Json replace(string path, object newValue)
    {
        if(newValue is string str)
            return new Json(ReplacePath(Value, path, str.Typed()));

        return new Json(ReplacePath(Value, path, newValue));
    }

    public Json add(string name, object value)
    {
        return new Json(AddToRoot(Value, name, value));
    }

    public override string ToString()
    {
        return Value.ToString().Replace("\r\n", "\n");
    }

    private static JObject ReplacePath(JObject root, string path, object newValue)
    {
        if (root == null || path == null)
            throw new ArgumentNullException();

        JObject rootCopy = (JObject)root.DeepClone();
        foreach (var currentToken in rootCopy.SelectTokens(path))
        {
            if (currentToken == rootCopy)
            {
                rootCopy = JObject.FromObject(newValue);
            }
            else
            {
                JToken newToken = GetNewToken(currentToken, newValue);
                currentToken.Replace(newToken);
            }
        }

        return rootCopy;
    }

    private static JToken GetNewToken(JToken currentToken, object newValue)
    {
        if (currentToken is JValue)
            return JToken.FromObject(newValue);

        if(newValue is string str)
            return JToken.Parse(str.Trim());

        if(newValue is Json json)
            return json.Value;

        return JToken.FromObject(newValue);
    }

    private static JObject AddToRoot(JObject root, string name, object value)
    {
        JObject rootCopy = (JObject)root.DeepClone();
        rootCopy.Add(name, JToken.FromObject(value));
        return rootCopy;
    }
}