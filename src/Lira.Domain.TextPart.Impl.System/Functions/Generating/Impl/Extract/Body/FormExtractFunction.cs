using Lira.Domain.Extensions;
using Lira.Domain.Matching.Request.Matchers;
using Lira.Domain.TextPart.Utils;

namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Extract.Body;

class FormExtractFunction : WithArgumentFunction<string>, IBodyExtractFunction, IObjectTextPart
{
    public override string Name => "req.body.form";
    public override bool ArgumentIsRequired => true;
    
    private string _formParamName = "";

    public string? Extract(string? value) => BodyUtils.GetByForm(value, _formParamName);

    public dynamic? Get(RuleExecutingContext context) => Extract(context.Request.ReadBody());

    public override void SetArgument(string argument) => _formParamName = argument;
}
