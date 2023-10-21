using Lira.Domain.Extensions;
using Lira.Domain.TextPart.Utils;

namespace Lira.Domain.TextPart.Impl.CSharp.DynamicModel;

public class RequestModel
{
    private readonly RequestData _data;
    public BodyModel Body { get; }

    public RequestModel(RequestData data)
    {
        _data = data;
        Body = new BodyModel(data.ReadBody());
    }

    public string? Header(string name) => _data.GetHeader(name);

    public string? Query(string name) => _data.GetQueryParam(name);
    
    public string Path(string name) => _data.GetPathSegmentValue(name);
    
    public record BodyModel(string Value)
    {
        public string? All() => Value;
        public string? JPath(string jsonpath) => BodyUtils.GetByJPath(Value, jsonpath);
        public string? XPath(string xpath) => BodyUtils.GetByXPath(Value, xpath);
        public string? Form(string paramName) => BodyUtils.GetByForm(Value, paramName);
    }
}
