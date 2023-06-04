namespace SimpleMockServer.Domain.TextPart.Functions.Functions.Generating.Impl.Extract;
internal class Header : IObjectTextPart, IWithStringArgumentFunction
{
    public static string Name => "req.header";

    private string _headerName = "";

    public object? Get(RequestData request)
    {
        if (request.Headers.TryGetValue(_headerName, out var result))
            return result.First();
        return null;
    }

    public void SetArgument(string argument)
    {
        _headerName = argument;
    }
}
