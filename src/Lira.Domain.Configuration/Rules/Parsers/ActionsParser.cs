using Lira.Common.Extensions;
using Lira.Domain.Actions;
using Lira.Domain.Configuration.Rules.Parsers.CodeParsing;
using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.Domain.TextPart.Impl.CSharp;
using Lira.FileSectionFormat;
using Lira.FileSectionFormat.Extensions;

namespace Lira.Domain.Configuration.Rules.Parsers;

class ActionsParser
{
    private readonly IEnumerable<ISystemActionRegistrator> _externalCallerRegistrators;
    private readonly IFunctionFactoryCSharp _functionFactoryCSharp;
    private readonly DelayGeneratorParser _delayGeneratorParser;

    public ActionsParser(IEnumerable<ISystemActionRegistrator> externalCallerRegistrators, IFunctionFactoryCSharp functionFactoryCSharp, DelayGeneratorParser delayGeneratorParser)
    {
        _externalCallerRegistrators = externalCallerRegistrators;
        _functionFactoryCSharp = functionFactoryCSharp;
        _delayGeneratorParser = delayGeneratorParser;
    }

    public IReadOnlySet<string> GetSectionNames(IReadOnlyCollection<FileSection> sections)
    {
        return sections
            .Select(s => s.Name)
            .Where(name => name.StartsWith(Constants.SectionName.ActionPrefix))
            .ToHashSet();
    }

    private static string GetSectionName(ISystemActionRegistrator registrator) => Constants.SectionName.ActionPrefix + "." + registrator.Name;

    internal async Task<IReadOnlyCollection<Delayed<IAction>>> Parse(IReadOnlyCollection<FileSection> sections, ParsingContext parsingContext)
    {
        var result = new List<Delayed<IAction>>();

        foreach (var section in sections)
        {
            if(!GetSectionNames(sections).Contains(section.Name))
                continue;

            var action = await GetAction(parsingContext, section);
            var getDelay = await _delayGeneratorParser.Parse(section, parsingContext);

            result.Add(new Delayed<IAction>(action, getDelay));
        }

        return result;
    }

    private async Task<IAction> GetAction(ParsingContext parsingContext, FileSection section)
    {
        IAction action;
        var registrator = _externalCallerRegistrators.FirstOrDefault(r => GetSectionName(r) == section.Name);

        if (registrator != null)
        {
            action = await registrator.Create(section, parsingContext);
        }
        else
        {
            var code = GetActionCode(section);
            var (codeBlock, newRuntimeVariables) = CodeParser.Parse(code, parsingContext.DeclaredItems);
            parsingContext.DeclaredItems.Variables.AddRange(newRuntimeVariables);

            var res = _functionFactoryCSharp.TryCreateAction(new DeclaredPartsProvider(parsingContext.DeclaredItems), codeBlock);
            action = res.GetFunctionOrThrow(code, parsingContext);
        }

        return action;
    }

    private static string GetActionCode(FileSection section)
    {
        string code;
        if (section.ChildSections.Count == 0)
        {
            var lines = section.LinesWithoutBlock;
            if (lines.Count == 0)
                throw new Exception($"Section '{section.Name}' is empty");

            code = lines.JoinWithNewLine();
        }
        else
        {
            var codeBlock = section.GetBlockRequired(Constants.BlockName.Action.Code);

            code = codeBlock.GetLinesAsString();
        }

        return code;
    }
}
