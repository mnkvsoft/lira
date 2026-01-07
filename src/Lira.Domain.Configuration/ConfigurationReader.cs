using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Lira.Domain.Configuration.RangeModel;
using Lira.Domain.Configuration.Rules;
using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using System.Diagnostics;
using Lira.Common;
using Lira.Common.State;
using Lira.Domain.TextPart.Impl.CSharp;
using Lira.Domain.TextPart.Impl.System;
using Lira.Domain.Configuration.CustomDictionaries;
using Lira.Domain.Configuration.DeclarationItems;

namespace Lira.Domain.Configuration;

class ConfigurationReader(
    RangesLoader rangesLoader,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<ConfigurationReader> logger,
    Factory<IRequestHandlerBuilder> requestHandlerBuilderFactory)
{
    public async Task<(IRequestHandler RequestHandler, IReadOnlyCollection<IStateful> States)> Read(string path)
    {
        logger.LogInformation("Loading rules...");
        var sw = Stopwatch.StartNew();

        var context = new ParsingContext(
            new DeclaredItems(),
            FunctionFactoryUsingContext.Empty,
            rootPath: path,
            currentPath: path);

        using var scope = serviceScopeFactory.CreateScope();
        var provider = scope.ServiceProvider;

        var dictsProvider = provider.GetRequiredService<CustomDictsProvider>();
        dictsProvider.Dicts = await CustomDictsLoader.Load(path);

        var ranges = await rangesLoader.Load(path);
        var rulesProvider = provider.GetRequiredService<RangesProvider>();
        rulesProvider.Ranges = ranges;

        var globalVariablesParser = provider.GetRequiredService<DeclaredItemsLoader>();

        var variables = await globalVariablesParser.Load(context, path);
        context.SetDeclaredItems(variables);

        var rulesLoader = provider.GetRequiredService<RulesLoader>();
        var requestHandlerBuilder = requestHandlerBuilderFactory();
        await rulesLoader.LoadRulesDatas(requestHandlerBuilder, path, context);

        logger.LogInformation($"{requestHandlerBuilder.Count} rules were successfully loaded ({(int)sw.ElapsedMilliseconds} ms)");
        var requestHandler = requestHandlerBuilder.Build();

        var sequence = provider.GetRequiredService<SystemSequence>();
        var rangesStates = ranges.SelectMany(x => x.Value.GetStates());

        var states = new Dictionary<string, IStateful>();
        foreach (var state in rangesStates.Union([sequence]))
        {
            if(!states.TryAdd(state.StateId, state))
                throw new Exception($"Duplicate state '{state.StateId}'");
        }

        return (requestHandler, states.Select(x=> x.Value).ToArray());
    }
}