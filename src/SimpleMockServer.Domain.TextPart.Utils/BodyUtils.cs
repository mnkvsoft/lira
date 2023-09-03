using System.Web;
using System.Xml;
using Newtonsoft.Json.Linq;

namespace SimpleMockServer.Domain.TextPart.Utils;

public static class BodyUtils
{
    public static string? GetByXPath(string? body, string xpath)
    {
        if (body == null)
            return null;

        var doc = new XmlDocument();

        try
        {
            doc.LoadXml(body);
        }
        catch
        {
            return null;
        }

        var nodes = doc.SelectNodes(xpath);

        var count = nodes?.Count ?? 0;

        if (count == 0)
            return null;

        if (count > 1)
        {
            throw new Exception($"Xpath '{xpath}' returns {count} elems. Xml: '{body}'");
        }

        var result = nodes![0]!.Value;
        return result;
    }
    
    public static string? GetByJPath(string? body, string jpath)
    {
        if (body == null)
            return null;

        JObject o;
        try
        {
            o = JObject.Parse(body);
        }
        catch
        {
            return null;
        }

        var token = o.SelectToken(jpath);
        var result = token?.Value<string>();
        return result;
    }
    
    public static string? GetByForm(string? value, string paramName)
    {
        if (value == null)
            return null;

        System.Collections.Specialized.NameValueCollection pars;
        try
        {
            pars = HttpUtility.ParseQueryString(value);
        }
        catch
        {
            return null;
        }

        var result = pars[paramName];
        return result;
    }
}