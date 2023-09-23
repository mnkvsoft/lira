namespace SimpleMockServer.Domain.TextPart.Impl.PreDefinedFunctions;

class UnknownFunctionException : Exception
{
    public UnknownFunctionException(string invoke) : base($"Unknown function invoke: '{invoke}'") { }
}
