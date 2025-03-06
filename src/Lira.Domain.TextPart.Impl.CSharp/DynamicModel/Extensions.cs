using System.Dynamic;
using System.Reflection;
using Lira.Domain.TextPart.Types;

namespace Lira.Domain.TextPart.Impl.CSharp.DynamicModel;

public static class Extensions
{
    public static Json json(this string str)
    {
        return Json.Parse(str);
    }

    public static ExpandoObject obj(this object initialObj)
    {
        ExpandoObject obj = new ExpandoObject();
        IDictionary<string, object> dic = obj;
        Type tipo = initialObj.GetType();
        foreach(var prop in tipo.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            dic.Add(prop.Name, prop.GetValue(initialObj));
        }
        return obj;
    }

    private static void Set(this ExpandoObject obj, string propertyName, object value)
    {
        IDictionary<string, object> dic = obj;
        dic[propertyName] = value;
    }
}