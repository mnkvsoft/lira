namespace SimpleMockServer.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Generating.Impl.Extract;

internal class Path : IObjectTextPart, IWithStringArgumentFunction
{
    public static string Name => "req.path";

    private string _segmentName = "";

    public object Get(RequestData request) => request.GetPathSegmentValue(_segmentName);

    public void SetArgument(string argument)
    {
        _segmentName = argument;
    }
}
