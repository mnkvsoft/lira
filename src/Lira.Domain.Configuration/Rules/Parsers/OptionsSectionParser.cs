using System.Collections.Immutable;
using Lira.Common;
using Lira.FileSectionFormat;
using Lira.FileSectionFormat.Extensions;

namespace Lira.Domain.Configuration.Rules.Parsers;

class Options
{
    [ParameterName("name")]
    public string? RuleName { get; init; }

    [ParameterName("history")]
    public bool HistoryEnabled { get; init; }
}

static class OptionsSectionParser
{
    public static Options? Parse(IImmutableList<FileSection> sections)
    {
        var configSections = sections.Where(s => s.Name == Constants.SectionName.Options).ToArray();
        if (configSections.Length > 1)
            throw new Exception($"There can only be one section '{Constants.SectionName.Options}'");

        if (configSections.Length == 0)
            return null;

        var configSection = configSections.Single();
        if (configSection.Blocks.Count > 0)
            throw new Exception($"Section '{Constants.SectionName.Options}' cannot contain child blocks. Blocks: {string.Join(", ", configSection.Blocks.Select(x=> x.Name))}");

        var lines = configSection.GetNotEmptyLinesWithoutBlock().ToArray();

        if (lines.Length == 0)
            return null;

        return LinesDeserializer.Deserialize<Options>(lines);
    }
}
