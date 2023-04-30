﻿using SimpleMockServer.ConfigurationProviding.Rules;

namespace SimpleMockServer.ConfigurationProviding;

public static class HttpMethodExtensions
{
    public static HttpMethod ToHttpMethod(this string method)
    {
        if (!Constants.HttpMethods.Contains(method))
            throw new Exception($"String '{method}' not http method");

        return new HttpMethod(method);
    }
}
