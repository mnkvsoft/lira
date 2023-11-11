namespace Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Generating.Impl.Extract;

internal class Query : WithArgumentFunction<string>, IObjectTextPart
{
    public override string Name => "req.query";
    public override bool ArgumentIsRequired => true;
    
    private string _queryParamName = "";

    public object? Get(RequestData request) => request.GetQueryParam(_queryParamName);


    public override void SetArgument(string argument)
    {
        _queryParamName = argument;
    }
}
