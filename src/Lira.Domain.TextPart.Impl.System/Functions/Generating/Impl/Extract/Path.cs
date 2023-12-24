namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Extract;

internal class Path : WithArgumentFunction<string>, IObjectTextPart
{
    public override string Name => "req.path";
    public override bool ArgumentIsRequired => true;
    private string _segmentName = "";

    public object Get(RequestData request) => request.GetPathSegmentValue(_segmentName);


    public override void SetArgument(string argument)
    {
        _segmentName = argument;
    }
}
