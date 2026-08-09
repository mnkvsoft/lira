using System.Dynamic;
using Lira.Common.Extensions;
using Lira.Domain.Extensions;
using Lira.Domain.TextPart.Utils;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace Lira.Domain.TextPart.Impl.CSharp.DynamicModel;

public class RequestModel
{
    private readonly RequestData _data;
    public dynamic body { get; }

    public RequestModel(RequestData data)
    {
        _data = data;
        body = new BodyModel(data.ReadBody(), data.GetHeader(HeaderNames.ContentType));
    }

    public string? header(string name) => _data.GetHeader(name);

    public string? query(string name) => _data.GetQueryParam(name);

    public string? path(int index) => _data.GetPath(index);
    public string path() => _data.Path;
    public string method() => _data.Method;

    public class BodyModel(string value, string? contentType) : DynamicObject
    {
        public string all() => value;
        public string? jpath(string jsonPath) => BodyUtils.GetByJPath(value, jsonPath);
        public string? xpath(string xPath) => BodyUtils.GetByXPath(value, xPath);
        public string? form(string paramName) => BodyUtils.GetByForm(value, paramName);

        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {
            if(string.IsNullOrWhiteSpace(value))
                throw new Exception("The body is empty");

            if (contentType?.Contains("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase) == true)
            {
                result = form(binder.Name);
                return true;
            }

            if (contentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true)
            {
                result = JObject.Parse(value).GetFieldValue(binder);
                return true;
            }

            throw new Exception(
                $$"""
                  Unable to access body field as an object. Unknown data format (ContentType: {{contentType}}):
                  {{value}}
                  """);
        }
    }
}