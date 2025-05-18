using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Lira.Domain.Configuration.RangeModel;
using Lira.Domain.Configuration.Rules;
using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.Domain.Configuration.Variables;
using System.Diagnostics;
using Lira.Common.State;
using Lira.Domain.TextPart.Impl.CSharp;
using Lira.Domain.TextPart.Impl.System;
using Lira.Domain.Configuration.CustomDictionaries;

namespace Lira.Domain.Configuration;

class ConfigurationReader(
    RangesLoader rangesLoader,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<ConfigurationReader> logger)
{
    public async Task<(IReadOnlyCollection<Rule> Rules, IReadOnlyCollection<IStateful> States)> Read(string path)
    {
        logger.LogInformation("Loading rules...");
        var sw = Stopwatch.StartNew();

        using var scope = serviceScopeFactory.CreateScope();
        var provider = scope.ServiceProvider;

        var customDicts = await CustomDictsLoader.Load(path);

        var context = new ParsingContext(
            provider.GetRequiredService<DeclaredItemsRegistry>(),
            FunctionFactoryUsingContext.Empty,
            customDicts,
            rootPath: path,
            currentPath: path);

        var ranges = await rangesLoader.Load(path);
        var rulesProvider = provider.GetRequiredService<RangesProvider>();
        rulesProvider.Ranges = ranges;

        var globalDeclaredItemsParser = provider.GetRequiredService<DeclaredItemsLoader>();

        var declaredItemsDrafts = await globalDeclaredItemsParser.ReadDrafts(path);
        context.DeclaredItemsRegistry.AddDraftsRange(declaredItemsDrafts);

        var rulesLoader = provider.GetRequiredService<RulesLoader>();
        var rules = await rulesLoader.LoadRules(path, context);

        logger.LogInformation($"{rules.Count} rules were successfully loaded ({(int)sw.ElapsedMilliseconds} ms)");

        var sequence = provider.GetRequiredService<SystemSequence>();
        var rangesStates = ranges.SelectMany(x => x.Value.GetStates());

        var states = new Dictionary<string, IStateful>();
        foreach (var state in rangesStates.Union([sequence]))
        {
            if(!states.TryAdd(state.StateId, state))
                throw new Exception($"Duplicate state '{state.StateId}'");
        }

        return (rules, states.Select(x=> x.Value).ToArray());
    }
}