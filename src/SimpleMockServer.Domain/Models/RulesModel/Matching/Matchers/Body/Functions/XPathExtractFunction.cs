using System.Xml;
using Microsoft.Extensions.Logging;

namespace SimpleMockServer.Domain.Models.RulesModel.Matching.Matchers.Body.Functions;

public class XPathExtractFunction : IExtractFunction
{
    private readonly string _xpath;
    private readonly ILogger _logger;

    public XPathExtractFunction(ILoggerFactory loggerFactory, string xpath)
    {
        _logger = loggerFactory.CreateLogger(GetType());
        _xpath = xpath;
    }

    public string? Extract(string? value)
    {
        if (value == null)
            return null;

        XmlDocument doc = new XmlDocument();

        try
        {
            doc.LoadXml(value);
        }
        catch
        {
            _logger.WarnAboutDecreasePerformance("xml", value);
            return null;
        }

        XmlNodeList? nodes = doc.SelectNodes(_xpath);

        int count = nodes?.Count ?? 0;

        if (count == 0)
            return null;

        if (count > 1)
        {
            throw new Exception($"Xpath '{_xpath}' returns {count} elems. Xml: '{value}'");
        }

        string? result = nodes![0]!.Value;
        return result;
    }
}