using SimpleMockServer.Common.Extensions;
using SimpleMockServer.Domain.TextPart.Variables;
using SimpleMockServer.FileSectionFormat;

namespace SimpleMockServer.Domain.Configuration.Rules.Parsers;

class ExternalCallerParser
{
    private readonly IEnumerable<IExternalCallerRegistrator> _externalCallerRegistrators;

    public ExternalCallerParser(IEnumerable<IExternalCallerRegistrator> externalCallerRegistrators)
    {
        _externalCallerRegistrators = externalCallerRegistrators;
    }

    public Dictionary<string, IReadOnlySet<string>> GetSectionsKnowsBlocks()
    {
        var result = new Dictionary<string, IReadOnlySet<string>>();

        foreach (var registrator in _externalCallerRegistrators)
        {
            var sectionName = GetSectionName(registrator);
            var knownBlocks = new HashSet<string>
            {
                Constants.BlockName.Common.Delay
            };

            knownBlocks.AddRangeOrThrowIfContains(registrator.GetSectionKnowsBlocks());

            result.Add(sectionName, knownBlocks);
        }

        return result;
    }

    private static string GetSectionName(IExternalCallerRegistrator registrator) => Constants.SectionName.CallPrefix + "." + registrator.Name;

    internal IReadOnlyCollection<Delayed<IExternalCaller>> Parse(IReadOnlyCollection<FileSection> sections, IReadOnlyCollection<Variable> variables)
    {
        var result = new List<Delayed<IExternalCaller>>();

        foreach (var section in sections)
        {
            var registrator = _externalCallerRegistrators.FirstOrDefault(registrator => GetSectionName(registrator) == section.Name);

            if (registrator == null)
                continue;

            var caller = registrator.Create(section, variables);

            TimeSpan? delay = null;
            var delayStr = section.GetStringValueFromBlockOrEmpty(Constants.BlockName.Common.Delay);

            if (!string.IsNullOrWhiteSpace(delayStr))
                delay = PrettyTimespanParser.Parse(delayStr);

            result.Add(new Delayed<IExternalCaller>(caller, delay));
        }

        return result;
    }
}
