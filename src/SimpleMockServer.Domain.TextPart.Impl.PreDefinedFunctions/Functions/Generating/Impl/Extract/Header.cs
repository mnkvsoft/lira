namespace SimpleMockServer.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Generating.Impl.Extract;
internal class Header : IObjectTextPart, IWithStringArgumentFunction
{
    public static string Name => "req.header";

    private string _headerName = "";

    public object? Get(RequestData request) => request.GetHeader(_headerName);

    public void SetArgument(string argument)
    {
        _headerName = argument;
    }
}
