namespace SimpleMockServer.Domain.TextPart.Functions.Functions.Generating.Impl.Extract;
internal class Header : IGeneratingPrettyFunction, IWithStringArgumenFunction
{
    public static string Name => "extract.header";

    private string _headerName;

    public object? Generate(RequestData request)
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
