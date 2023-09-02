namespace SimpleMockServer.Domain.TextPart.System.Functions.Functions;

internal interface IWithStringArgumentFunction : IWithArgument
{
    void SetArgument(string argument);
}