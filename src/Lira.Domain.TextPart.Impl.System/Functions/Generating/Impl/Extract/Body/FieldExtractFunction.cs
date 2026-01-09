using System.Text.Json.Nodes;
using System.Web;
using Lira.Common;
using Lira.Domain.Matching.Request.Matchers;

namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Extract.Body;

public class FieldExtractFunction(ObjectPath path) : IBodyExtractFunction
{
    public string? Extract(string? body)
    {
        if (string.IsNullOrEmpty(body))
            return null;

        if (JsonUtils.MaybeValidJson(body))
        {
            try
            {
                var node = JsonNode.Parse(body)!;
                path.TryGetStringValue(node, out var value);
                return value;
            }
            catch
            {
                return null;
            }
        }

        if (path.Elems.Count != 1 || path.Elems.First() is not ObjectPath.Elem.Field field)
            return null;

        var pars = HttpUtility.ParseQueryString(body);
        return pars[field.Name];
    }
}