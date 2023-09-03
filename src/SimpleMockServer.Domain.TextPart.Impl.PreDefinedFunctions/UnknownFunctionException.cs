namespace SimpleMockServer.Domain.TextPart.PreDefinedFunctions;

class UnknownFunctionException : Exception
{
    public UnknownFunctionException(string invoke) : base($"Unknown function invoke: '{invoke}'") { }
}
