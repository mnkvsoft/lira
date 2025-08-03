using System.Dynamic;
using Lira.Common.Exceptions;
using Newtonsoft.Json.Linq;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

namespace Lira.Domain.TextPart.Types;

public class Json : DynamicObject
{
    private JObject _jObject;

    public static Json Parse(string value)
    {
        return new Json(JObject.Parse(value));
    }

    private Json(JObject jObject)
    {
        _jObject = jObject;
    }

    public Json copy()
    {
        return new Json((JObject)_jObject.DeepClone());
    }

    public Json replace(string path, object newValue)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentNullException(nameof(path) + " is empty");

        foreach (var currentToken in _jObject.SelectTokens(path))
        {
            if (currentToken == _jObject)
            {
                _jObject = JObject.FromObject(newValue);
            }
            else
            {
                JToken newToken = GetNewToken(newValue);
                currentToken.Replace(newToken);
            }
        }

        return this;
    }

    public Json remove(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentNullException(nameof(path) + " is empty");

        foreach (var tokenToRemove in _jObject.SelectTokens(path))
        {
            if (tokenToRemove == _jObject)
            {
                throw new InvalidOperationException("Cannot remove the root object");
            }

            if (tokenToRemove.Parent is JProperty property)
            {
                property.Remove();
            }
            else if (tokenToRemove.Parent is JArray)
            {
                tokenToRemove.Remove();
            }
        }

        return this;
    }

    public Json add(string path, params object[] values)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentNullException(nameof(path) + " is empty");

        var tokens = _jObject.SelectTokens(path).ToArray();

        if (tokens.Length > 1)
            throw new Exception(
                $"Failed to add new element to array. Path must be return one element only. Path: {path}. Selected elements: {string.Join(", ", tokens.Select(x => x.Path))}");

        var token = tokens.Single();

        if (token is JArray array)
        {
            foreach (var value in values)
            {
                array.Add(GetNewToken(value));
            }
        }

        if (token is JObject obj)
        {
            if (values.Length > 2)
            {
                throw new Exception(
                    $"Failed to add a new field to an object. It is not possible to add more than one element to an object. Path: {path}. Values: {string.Join(", ", values.Skip(1))}");
            }

            if (values.Length < 2)
            {
                throw new Exception(
                    $"Failed to add a new field to an object. Required to specify a value for a new object field. Path: {path}");
            }

            object nameObj = values[0];

            string name;

            if (nameObj is JValue jValue)
            {
                name = jValue.Value?.ToString()
                       ?? throw new Exception($"Failed to add a new field to an object. The new field name must be not null");
            }
            else if (nameObj is string stringValue)
            {
                name = stringValue;
            }
            else
            {
                throw new Exception(
                    $"Failed to add a new field to an object. The new field name must be a string. Current type: {nameObj.GetType()}");
            }
            var value = values[1];

            obj.Add(name, GetNewToken(value));
        }

        return this;
    }

    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        result = true;
        var name = binder.Name;
        if (string.IsNullOrEmpty(name))
            throw new Exception("Name is empty");

        var token = _jObject.GetValue(name);

        if (token == null)
            throw new Exception($"Field '{name}' not found");

        if (token is JValue value)
        {
            result = value.Value;
            return true;
        }

        if (token is JObject obj)
        {
            result = obj;
            return true;
        }

        if (token is JArray array)
        {
            result = array;
            return true;
        }

        throw new Exception($"Unknown token type for '{name}': " + token);
    }

    private static JToken GetNewToken(object newValue)
    {
        if (newValue is Json json)
            return json._jObject;

        if (newValue is string str && (str.StartsWith('{') || str.StartsWith('[')))
        {
            return JToken.Parse(str.Trim());
        }

        return JToken.FromObject(newValue);
    }

    public override string ToString()
    {
        return _jObject.ToString().Replace("\r\n", "\n");
    }
}