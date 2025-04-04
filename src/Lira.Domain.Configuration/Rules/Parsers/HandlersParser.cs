using Lira.Common.Extensions;
using Lira.Domain.Configuration.Rules.Parsers.CodeParsing;
using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.Domain.Handling.Actions;
using Lira.Domain.TextPart.Impl.CSharp;
using Lira.FileSectionFormat;
using Lira.FileSectionFormat.Extensions;

namespace Lira.Domain.Configuration.Rules.Parsers;

class HandlersParser
{
    private readonly IEnumerable<ISystemActionRegistrator> _externalCallerRegistrators;
    private readonly IFunctionFactoryCSharpFactory _functionFactoryCSharpFactory;
    private readonly GetDelayParser _getDelayParser;
    private readonly ResponseGenerationHandlerParser _responseGenerationHandlerParser;

    public HandlersParser(IEnumerable<ISystemActionRegistrator> externalCallerRegistrators, IFunctionFactoryCSharpFactory functionFactoryCSharpFactory, GetDelayParser getDelayParser, ResponseGenerationHandlerParser responseGenerationHandlerParser)
    {
        _externalCallerRegistrators = externalCallerRegistrators;
        _functionFactoryCSharpFactory = functionFactoryCSharpFactory;
        _getDelayParser = getDelayParser;
        _responseGenerationHandlerParser = responseGenerationHandlerParser;
    }

    public IReadOnlySet<string> GetAllSectionNames(IReadOnlyCollection<FileSection> sections)
    {
        return GetActionSectionNames(sections)
            .Union([Constants.SectionName.Response])
            .ToHashSet();
    }

    private static IEnumerable<string> GetActionSectionNames(IReadOnlyCollection<FileSection> sections)
    {
        return sections
            .Select(s => s.Name)
            .Where(name => name.StartsWith(Constants.SectionName.ActionPrefix));
    }

    private static string GetSectionName(ISystemActionRegistrator registrator) => Constants.SectionName.ActionPrefix + "." + registrator.Name;

    internal async Task<IReadOnlyCollection<Delayed<IHandler>>> Parse(IReadOnlyCollection<FileSection> sections, ParsingContext parsingContext)
    {
        var result = new List<Delayed<IHandler>>();

        var sectionNames = GetAllSectionNames(sections);
        var actionNames = GetActionSectionNames(sections).ToArray();

        foreach (var section in sections.Where(s => sectionNames.Contains(s.Name)))
        {
            if(!GetAllSectionNames(sections).Contains(section.Name))
                continue;

            IHandler action;
            if (actionNames.Contains(section.Name))
            {
                action = await GetAction(parsingContext, section);
            }
            else if(section.Name == Constants.SectionName.Response)
            {
                action = await _responseGenerationHandlerParser.Parse(section, parsingContext);
            }
            else
            {
                throw new Exception($"Unknown section: {section.Name}");
            }

            var getDelay = await _getDelayParser.Parse(section, parsingContext);

            result.Add(new Delayed<IHandler>(action, getDelay));
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
            parsingContext.DeclaredItems.Variables.TryAddRuntimeVariables(newRuntimeVariables);

            var functionFactory = await _functionFactoryCSharpFactory.Get();
            var res = functionFactory.TryCreateAction(new DeclaredPartsProvider(parsingContext.DeclaredItems), codeBlock);
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
