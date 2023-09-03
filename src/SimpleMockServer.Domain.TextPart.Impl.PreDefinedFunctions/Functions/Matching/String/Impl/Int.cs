﻿using SimpleMockServer.Domain.Matching.Request;

namespace SimpleMockServer.Domain.TextPart.PreDefinedFunctions.Functions.Matching.String.Impl;

internal class Int : IStringMatchPrettyFunction
{
    public static string Name => "int";

    public MatchFunctionRestriction Restriction => MatchFunctionRestriction.Type;

    public bool IsMatch(string? value)
    {
        return long.TryParse(value, out _);
    }
}
