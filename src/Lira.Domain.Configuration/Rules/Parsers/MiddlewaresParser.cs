using Lira.Common.Extensions;
using Lira.Domain.Configuration.Rules.Parsers.CodeParsing;
using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.Domain.Handling;
using Lira.Domain.Handling.Generating;
using Lira.Domain.TextPart.Impl.CSharp;
using Lira.FileSectionFormat;
using Lira.FileSectionFormat.Extensions;

namespace Lira.Domain.Configuration.Rules.Parsers;

class MiddlewaresParser
{
    private readonly IEnumerable<ISystemActionRegistrator> _externalCallerRegistrators;
    private readonly IFunctionFactoryCSharpFactory _functionFactoryCSharpFactory;
    private readonly GetDelayParser _getDelayParser;
    private readonly ResponseGenerationHandlerParser _responseGenerationHandlerParser;
    private readonly CodeParser _codeParser;

    public MiddlewaresParser(IEnumerable<ISystemActionRegistrator> externalCallerRegistrators,
        IFunctionFactoryCSharpFactory functionFactoryCSharpFactory, GetDelayParser getDelayParser,
        ResponseGenerationHandlerParser responseGenerationHandlerParser, CodeParser codeParser)
    {
        _externalCallerRegistrators = externalCallerRegistrators;
        _functionFactoryCSharpFactory = functionFactoryCSharpFactory;
        _getDelayParser = getDelayParser;
        _responseGenerationHandlerParser = responseGenerationHandlerParser;
        _codeParser = codeParser;
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

    private static string GetSectionName(ISystemActionRegistrator registrator) =>
        Constants.SectionName.ActionPrefix + "." + registrator.Name;


    record MiddlewareBuilder
    {
        public Delayed<IMiddleware>? Action = null;
        public List<Delayed<IMiddleware>> Handlers = new();
    }

    public async Task<IReadOnlyCollection<Func<Delayed<IMiddleware>>>> Parse(WriteHistoryMode writeHistoryMode,
        IReadOnlyCollection<FileSection> sections, ParsingContext parsingContext)
    {
        var builders = new List<MiddlewareBuilder>();

        var sectionNames = GetAllSectionNames(sections);
        var actionNames = GetActionSectionNames(sections).ToArray();

        MiddlewareBuilder? handlerBuilder = null;

        foreach (var section in sections.Where(s => sectionNames.Contains(s.Name)))
        {
            var getDelay = await _getDelayParser.Parse(section, parsingContext);

            if (actionNames.Contains(section.Name))
            {
                var action = await GetAction(parsingContext, section);

                builders.Add(new MiddlewareBuilder
                {
                    Action = new Delayed<IMiddleware>(action, getDelay)
                });
            }
            else if (section.Name == Constants.SectionName.Response)
            {
                var handler = await _responseGenerationHandlerParser.Parse(writeHistoryMode, section, parsingContext);
                if (handlerBuilder == null)
                {
                    handlerBuilder = new MiddlewareBuilder();
                    builders.Add(handlerBuilder);
                }

                handlerBuilder.Handlers.Add(new Delayed<IMiddleware>(handler, getDelay));
            }
            else
            {
                throw new Exception($"Unknown section: {section.Name}");
            }
        }

        return builders.Select(GetMiddleware).ToArray();

        static Func<Delayed<IMiddleware>> GetMiddleware(MiddlewareBuilder builder)
        {
            if (builder.Action != null)
                return () => builder.Action;

            if (builder.Handlers.Count == 0)
                throw new Exception("No handlers have been configured for rule");

            if (builder.Handlers.Count == 1)
                return () => builder.Handlers.First();

            return () => builder.Handlers.Random();
        }
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
            var (codeBlock, newRuntimeVariables, newLocalVariables) =
                _codeParser.Parse(code, parsingContext.DeclaredItems);

            parsingContext.DeclaredItems.TryAddRange(newRuntimeVariables);
            parsingContext.DeclaredItems.TryAddRange(newLocalVariables);

            var functionFactory = await _functionFactoryCSharpFactory.Get();
            var res = functionFactory.TryCreateAction(
                new FunctionFactoryRuleContext(parsingContext.CSharpUsingContext,
                    new DeclaredItemsProvider(parsingContext.DeclaredItems)),
                codeBlock);
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
            var codeBlock = section.GetBlockRequired(Constants.BlockName.Action.Script);

            code = codeBlock.GetLinesAsString();
        }

        return code;
    }
}