using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Lira.Domain.Configuration.RangeModel;
using Lira.Domain.Configuration.Rules;
using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.Domain.Configuration.Templating;
using Lira.Domain.Configuration.Variables;
using System.Diagnostics;
using Lira.Common.State;
using Lira.Domain.Configuration.CustomSets;
using Lira.Domain.TextPart.Impl.System;

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

        var templates = await TemplatesLoader.Load(path);
        var customDicts = await CustomDictsLoader.Load(path);

        var context = new ParsingContext(
            new DeclaredItems(),
            templates,
            customDicts,
            rootPath: path,
            currentPath: path);

        using var scope = serviceScopeFactory.CreateScope();
        var provider = scope.ServiceProvider;

        var ranges = await rangesLoader.Load(path);
        var rulesProvider = provider.GetRequiredService<RangesProvider>();
        rulesProvider.Ranges = ranges;

        var globalVariablesParser = provider.GetRequiredService<DeclaredItemsLoader>();

        var variables = await globalVariablesParser.Load(context, path);
        context.SetDeclaredItems(variables);

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