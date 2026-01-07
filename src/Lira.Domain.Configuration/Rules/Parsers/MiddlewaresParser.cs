using Lira.Common;
using Lira.Common.Extensions;
using Lira.Domain.Configuration.Rules.Parsers.CodeParsing;
using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.Domain.Handling;
using Lira.Domain.Handling.Generating.ResponseStrategies;
using Lira.Domain.TextPart.Impl.CSharp;
using Lira.FileSectionFormat;
using Lira.FileSectionFormat.Extensions;

namespace Lira.Domain.Configuration.Rules.Parsers;

class MiddlewaresParser
{
    private readonly IEnumerable<ISystemActionRegistrator> _externalCallerRegistrators;
    private readonly IFunctionFactoryCSharpFactory _functionFactoryCSharpFactory;
    private readonly GetDelayParser _getDelayParser;
    private readonly ResponseStrategyParser _responseStrategyParser;
    private readonly CodeParser _codeParser;
    private readonly IMiddlewareFactory _middlewareFactory;

    public MiddlewaresParser(
        IEnumerable<ISystemActionRegistrator> externalCallerRegistrators,
        IFunctionFactoryCSharpFactory functionFactoryCSharpFactory,
        GetDelayParser getDelayParser,
        ResponseStrategyParser responseStrategyParser,
        CodeParser codeParser,
        IMiddlewareFactory middlewareFactory)
    {
        _externalCallerRegistrators = externalCallerRegistrators;
        _functionFactoryCSharpFactory = functionFactoryCSharpFactory;
        _getDelayParser = getDelayParser;
        _responseStrategyParser = responseStrategyParser;
        _codeParser = codeParser;
        _middlewareFactory = middlewareFactory;
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
        public Delayed<IAction>? Action;
        public readonly List<Delayed<IResponseStrategy>> Responses = new();
    }

    public async Task<IReadOnlyCollection<Factory<Delayed<Middleware>>>> Parse(
        ResponseMiddlewareModes modes,
        IReadOnlyCollection<FileSection> sections,
        ParsingContext parsingContext)
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
                    Action = new Delayed<IAction>(action, getDelay)
                });
            }
            else if (section.Name == Constants.SectionName.Response)
            {
                var responseStrategy = await _responseStrategyParser.Parse(section, parsingContext);
                if (handlerBuilder == null)
                {
                    handlerBuilder = new MiddlewareBuilder();
                    builders.Add(handlerBuilder);
                }

                handlerBuilder.Responses.Add(new Delayed<IResponseStrategy>(responseStrategy, getDelay));
            }
            else
            {
                throw new Exception($"Unknown section: {section.Name}");
            }
        }

        return builders.Select(builder => GetMiddleware(modes, builder)).ToArray();
    }

    Factory<Delayed<Middleware>> GetMiddleware(ResponseMiddlewareModes modes, MiddlewareBuilder builder)
    {
        if (builder.Action != null)
            return _middlewareFactory.CreateAction(builder.Action);

        if (builder.Responses.Count == 0)
            throw new Exception("No handlers have been configured for rule");

        if (builder.Responses.Count == 1)
        {
            var delayed = builder.Responses.First();
            return _middlewareFactory.CreateResponse(modes, delayed);
        }

        return _middlewareFactory.CreateMultipleResponse(modes, builder.Responses);
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