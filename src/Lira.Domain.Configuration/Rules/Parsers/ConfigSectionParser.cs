using System.Collections.Immutable;
using Lira.FileSectionFormat;
using Lira.FileSectionFormat.Extensions;

namespace Lira.Domain.Configuration.Rules.Parsers;

record Config(RuleName? RuleName, bool HistoryEnabled)
{
    public static class ParameterName
    {
        public const string RuleName = "name";
        public const string HistoryEnabled = "history";
    }
}

static class ConfigSectionParser
{
    public static Config? Parse(IImmutableList<FileSection> sections)
    {
        var configSections = sections.Where(s => s.Name == Constants.SectionName.Config).ToArray();
        if (configSections.Length > 1)
            throw new Exception($"There can only be one section '{Constants.SectionName.Config}'");

        if (configSections.Length == 0)
            return null;

        var configSection = configSections.Single();
        if (configSection.Blocks.Count > 0)
            throw new Exception($"Section '{Constants.SectionName.Config}' cannot contain child blocks. Blocks: {string.Join(", ", configSection.Blocks.Select(x=> x.Name))}");

        var lines = configSection.GetNotEmptyLinesWithoutBlock().ToArray();

        if (lines.Length == 0)
            return null;

        var nameToValueMap = new Dictionary<string, string>();
    }
}