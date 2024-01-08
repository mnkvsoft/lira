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
        foreach (var value in rootCopy.SelectTokens(path))
        {
            if (value == rootCopy)
                rootCopy = JObject.FromObject(newValue);
            else
                value.Replace(JToken.FromObject(newValue));
        }

        return rootCopy;
    }

    private static JObject AddToRoot(JObject root, string name, object value)
    {
        JObject rootCopy = (JObject)root.DeepClone();
        rootCopy.Add(name, JToken.FromObject(value));
        return rootCopy;
    }    
}