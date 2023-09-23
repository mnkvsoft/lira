namespace SimpleMockServer.Domain.TextPart.Impl.PreDefinedFunctions.Functions;

internal interface IWithStringArgumentFunction : IWithArgument
{
    void SetArgument(string argument);
}