namespace SimpleMockServer.Domain.TextPart.PreDefinedFunctions.Functions;

internal interface IWithStringArgumentFunction : IWithArgument
{
    void SetArgument(string argument);
}