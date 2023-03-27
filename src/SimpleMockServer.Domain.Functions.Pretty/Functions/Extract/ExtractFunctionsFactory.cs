using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using SimpleMockServer.Domain.Models.RulesModel.Matching.Matchers.Body;
using SimpleMockServer.Domain.Models.RulesModel.Matching.Matchers.Body.Functions;

namespace SimpleMockServer.Domain.Functions.Pretty.Functions.Extract;
internal class ExtractFunctionsFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly Dictionary<string, Func<string, IExtractFunction>> _functionsMap;

    public ExtractFunctionsFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;

        _functionsMap = new Dictionary<string, Func<string, IExtractFunction>>
        {
            {"jpath", arg => new JsonPathExtractFunction(_loggerFactory, jpath: arg)},
            {"xpath", arg => new XPathExtractFunction(_loggerFactory, xpath: arg)},
            {"form", arg => new XPathExtractFunction(_loggerFactory, xpath: arg)},
        };
    }

    public bool TryCreate(string value, [MaybeNullWhen(false)] out IExtractFunction? function)
    {
        function = null;

        if (value == "all")
        {
            function = new AllExtractFunction();
            return true;
        }

        foreach (var funcName in _functionsMap.Keys)
        {
            string funcNameStart = funcName + ":";
            if (value.StartsWith(funcNameStart))
            {
                string arg = value.Replace(funcNameStart, "").Trim();
                Func<string, IExtractFunction> funcFactory = _functionsMap[funcName];
                function = funcFactory(arg);
                return true;
            }
        }

        return false;
    }
}


