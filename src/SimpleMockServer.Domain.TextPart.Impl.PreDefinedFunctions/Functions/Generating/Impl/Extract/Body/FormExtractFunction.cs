using SimpleMockServer.Domain.Extensions;
using SimpleMockServer.Domain.Matching.Request.Matchers;
using SimpleMockServer.Domain.TextPart.Utils;

namespace SimpleMockServer.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Generating.Impl.Extract.Body;

public class FormExtractFunction : IBodyExtractFunction, IObjectTextPart, IWithStringArgumentFunction
{
    public static string Name => "req.body.form";
    
    private string _formParamName = "";

    public string? Extract(string? value) => BodyUtils.GetByForm(value, _formParamName);

    public object? Get(RequestData request) => Extract(request.ReadBody());

    public void SetArgument(string argument) => _formParamName = argument;
}
