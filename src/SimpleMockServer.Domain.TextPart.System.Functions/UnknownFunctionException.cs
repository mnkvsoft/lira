namespace SimpleMockServer.Domain.TextPart.System.Functions;

class UnknownFunctionException : Exception
{
    public UnknownFunctionException(string invoke) : base($"Unknown function invoke: '{invoke}'") { }
}
