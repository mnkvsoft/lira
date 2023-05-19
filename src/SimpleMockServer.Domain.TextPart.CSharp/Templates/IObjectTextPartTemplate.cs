using System;
using System.Text;
using System.Collections;
using System.Text.Json;

using SimpleMockServer.Domain;
using SimpleMockServer.Domain.TextPart;
using SimpleMockServer.Domain.TextPart.Variables;

namespace __DynamicGenerated;

public class className : IObjectTextPart
{
    private readonly IReadOnlyCollection<Variable> _variables;

    public className(IReadOnlyCollection<Variable> variables)
    {
        _variables = variables;
    }

    public object Get(RequestData request)
    {
        return 1;
    }

    private dynamic? GetVariable(string name, RequestData request)
    {
        return _variables.GetOrThrow(name).Get(request);
    }
}
