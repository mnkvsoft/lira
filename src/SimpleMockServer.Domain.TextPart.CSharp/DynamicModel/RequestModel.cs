using Microsoft.Extensions.Primitives;
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
    
    public string? Header(string name)
    {
        if (_data.Headers.TryGetValue(name, out StringValues values))
            return values.First();

        return null;
    }
    
    public string? Query(string name)
    {
        if (_data.Query.TryGetValue(name, out StringValues values))
            return values.First();

        return null;
    }
    
    public string? Path(string name)
    {
        throw new NotImplementedException();
    }
    
    public record BodyModel(string Value)
    {
        public string? All() => Value;
        public string? ByJPath(string jpath) => BodyUtils.GetByJPath(Value, jpath);
        public string? ByXPath(string xpath) => BodyUtils.GetByXPath(Value, xpath);
        public string? ByForm(string paramName) => BodyUtils.GetByForm(Value, paramName);
    }
}
