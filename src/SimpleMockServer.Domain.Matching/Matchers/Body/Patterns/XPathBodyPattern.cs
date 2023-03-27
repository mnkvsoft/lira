using System.Xml;
using Microsoft.Extensions.Logging;
using SimpleMockServer.Domain.Matching;
using SimpleMockServer.Domain.Matching.Matchers.Body;

namespace SimpleMockServer.Domain.Matching.Matchers.Body.Patterns;

class XPathBodyPattern : IBodyPattern
{
    private IReadOnlyDictionary<string, IValuePattern> _jsonPathToPattern;
    private readonly ILogger _logger;

    public XPathBodyPattern(ILoggerFactory loggerFactory, IReadOnlyDictionary<string, IValuePattern> jsonPathToPattern)
    {
        _jsonPathToPattern = jsonPathToPattern;
        _logger = loggerFactory.CreateLogger(GetType());
    }

    public bool IsMatch(string body)
    {
        XmlDocument doc = new XmlDocument();

        try
        {
            doc.LoadXml(body);
        }
        catch
        {
            _logger.WarnAboutDecreasePerformance("xml", body);
            return false;
        }

        foreach (var pair in _jsonPathToPattern)
        {
            var xpath = pair.Key;
            var pattern = pair.Value;

            XmlNodeList? nodes = doc.SelectNodes(xpath);

            int count = nodes?.Count ?? 0;
            if (count != 1)
            {
                throw new Exception($"Xpath '{xpath}' returns {count} elems. Xml: '{body}'");
            }

            string value = nodes![0]!.Value ?? "";

            if (!pattern.IsMatch(value))
                return false;
        }

        return true;
    }
}