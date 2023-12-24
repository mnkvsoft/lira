namespace Lira.Domain.TextPart.Impl.System;

class UnknownFunctionException : Exception
{
    public UnknownFunctionException(string invoke) : base($"Unknown function invoke: '{invoke}'") { }
}
