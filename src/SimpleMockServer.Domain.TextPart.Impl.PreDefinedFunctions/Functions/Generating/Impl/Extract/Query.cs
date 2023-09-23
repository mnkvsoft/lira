﻿namespace SimpleMockServer.Domain.TextPart.PreDefinedFunctions.Functions.Generating.Impl.Extract;

internal class Query : IObjectTextPart, IWithStringArgumentFunction
{
    public static string Name => "req.query";

    private string _queryParamName = "";

    public object? Get(RequestData request) => request.GetQueryParam(_queryParamName);

    public void SetArgument(string argument)
    {
        _queryParamName = argument;
    }
}