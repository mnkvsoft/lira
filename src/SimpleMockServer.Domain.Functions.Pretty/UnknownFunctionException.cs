namespace SimpleMockServer.Domain.Functions.Pretty;

class UnknownFunctionException : Exception
{
    public UnknownFunctionException(string invoke) : base($"Unknown function invoke: '{invoke}'") { }
}
