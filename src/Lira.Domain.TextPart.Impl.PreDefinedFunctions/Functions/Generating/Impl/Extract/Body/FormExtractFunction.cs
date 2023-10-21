using Lira.Domain.Extensions;
using Lira.Domain.Matching.Request.Matchers;
using Lira.Domain.TextPart.Utils;

namespace Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Generating.Impl.Extract.Body;

class FormExtractFunction : WithArgumentFunction<string>, IBodyExtractFunction, IObjectTextPart
{
    public static string Name => "req.body.form";
    public override bool ArgumentIsRequired => true;
    
    private string _formParamName = "";

    public string? Extract(string? value) => BodyUtils.GetByForm(value, _formParamName);

    public object? Get(RequestData request) => Extract(request.ReadBody());

    public override void SetArgument(string argument) => _formParamName = argument;
}
