using SimpleMockServer.Domain.Extensions;
using SimpleMockServer.Domain.TextPart.Functions.Utils;

namespace SimpleMockServer.Domain.TextPart.CSharp.DynamicModel;

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
        public string? ByJPath(string jpath) => BodyUtils.GetByJPath(Value, jpath);
        public string? ByXPath(string xpath) => BodyUtils.GetByXPath(Value, xpath);
        public string? ByForm(string paramName) => BodyUtils.GetByForm(Value, paramName);
    }
}
