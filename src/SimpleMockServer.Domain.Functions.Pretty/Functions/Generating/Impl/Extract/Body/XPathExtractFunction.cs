using System.Xml;
using Microsoft.Extensions.Logging;
using SimpleMockServer.Domain.Functions.Pretty.Functions.Generating.Impl.Extract.Body.Extensions;
using SimpleMockServer.Domain.Models.RulesModel;
using SimpleMockServer.Domain.Models.RulesModel.Matching.Request.Matchers.Body;

namespace SimpleMockServer.Domain.Functions.Pretty.Functions.Generating.Impl.Extract.Body;

public class XPathExtractFunction : IBodyExtractFunction, IGeneratingPrettyFunction, IWithStringArgumenFunction
{
    public static string Name => "extract.body.xpath";

    private string _xpath;
    private readonly ILogger _logger;

    public XPathExtractFunction(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(GetType());
    }

    public string? Extract(string? value)
    {
        if (value == null)
            return null;

        var doc = new XmlDocument();

        try
        {
            doc.LoadXml(value);
        }
        catch
        {
            _logger.WarnAboutDecreasePerformance("xml", value);
            return null;
        }

        var nodes = doc.SelectNodes(_xpath);

        var count = nodes?.Count ?? 0;

        if (count == 0)
            return null;

        if (count > 1)
        {
            throw new Exception($"Xpath '{_xpath}' returns {count} elems. Xml: '{value}'");
        }

        var result = nodes![0]!.Value;
        return result;
    }

    public object? Generate(RequestData request)
    {
        return Extract(request.ReadBody());
    }

    public void SetArgument(string argument)
    {
        _xpath = argument;
    }
}
