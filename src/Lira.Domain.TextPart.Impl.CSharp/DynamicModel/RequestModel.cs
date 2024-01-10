using Lira.Domain.Extensions;
using Lira.Domain.TextPart.Utils;

namespace Lira.Domain.TextPart.Impl.CSharp.DynamicModel;

public class RequestModel
{
    private readonly RequestData _data;
    public BodyModel body { get; }

    public RequestModel(RequestData data)
    {
        _data = data;
        body = new BodyModel(data.ReadBody());
    }

    public string? header(string name) => _data.GetHeader(name);

    public string? query(string name) => _data.GetQueryParam(name);
    
    public string path(string name) => _data.GetPathSegmentValue(name);
    
    public record BodyModel(string Value)
    {
        public string? all() => Value;
        public string? jpath(string jsonpath) => BodyUtils.GetByJPath(Value, jsonpath);
        public string? xpath(string xpath) => BodyUtils.GetByXPath(Value, xpath);
        public string? form(string paramName) => BodyUtils.GetByForm(Value, paramName);
    }
}
