using SimpleMockServer.Domain.Configuration.PrettyParsers;
using SimpleMockServer.Domain.Configuration.Rules.ValuePatternParsing;
using SimpleMockServer.FileSectionFormat;

namespace SimpleMockServer.Domain.Configuration.Rules.Parsers;

class ExternalCallerParser
{
    private readonly IEnumerable<IExternalCallerRegistrator> _externalCallerRegistrators;

    public ExternalCallerParser(IEnumerable<IExternalCallerRegistrator> externalCallerRegistrators)
    {
        _externalCallerRegistrators = externalCallerRegistrators;
    }

    public IReadOnlySet<string> GetSectionNames()
    {
        var result = new HashSet<string>();

        foreach (var registrator in _externalCallerRegistrators)
        {
            var sectionName = GetSectionName(registrator);
            result.Add(sectionName);
        }

        return result;
    }

    private static string GetSectionName(IExternalCallerRegistrator registrator) => Constants.SectionName.CallPrefix + "." + registrator.Name;

    internal async Task<IReadOnlyCollection<Delayed<IExternalCaller>>> Parse(IReadOnlyCollection<FileSection> sections, ParsingContext parsingContext)
    {
        var result = new List<Delayed<IExternalCaller>>();

        foreach (var section in sections)
        {
            var registrator = _externalCallerRegistrators.FirstOrDefault(registrator => GetSectionName(registrator) == section.Name);

            if (registrator == null)
                continue;

            var caller = await registrator.Create(section, parsingContext);

            TimeSpan? delay = null;
            var delayStr = section.GetStringValueFromBlockOrEmpty(Constants.BlockName.Common.Delay);

            if (!string.IsNullOrWhiteSpace(delayStr))
                delay = PrettyTimespanParser.Parse(delayStr);

            result.Add(new Delayed<IExternalCaller>(caller, delay));
        }

        return result;
    }
}
