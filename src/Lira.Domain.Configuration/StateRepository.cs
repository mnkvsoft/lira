using System.Collections.Immutable;
using Lira.Common;
using Microsoft.Extensions.Logging;
using Lira.Common.Extensions;

namespace Lira.Domain.Configuration;

public interface IStateRepository
{
    Task UpdateStates(IReadOnlyDictionary<string, string> states);
    Task<IReadOnlyDictionary<string, string>> GetStates();
}

class StateRepository(ILogger<StateRepository> logger) : IStateRepository
{
    private static readonly string StatesPath = Paths.GetTempSubPath("states");

    public async Task UpdateStates(IReadOnlyDictionary<string, string> states)
    {
        if(!Directory.Exists(StatesPath))
            Directory.CreateDirectory(StatesPath);

        foreach (var (stateId, stateValue) in states)
        {
            var path = Path.Combine(StatesPath, stateId + ".state");
            await File.WriteAllTextAsync(path, stateValue);
        }

        logger.LogInformation($"{states.Count} states have been saved to: {StatesPath}");
    }

    public async Task<IReadOnlyDictionary<string, string>> GetStates()
    {
        if(!Directory.Exists(StatesPath))
            return ImmutableDictionary<string, string>.Empty;

        var states = new Dictionary<string, string>();
        foreach (var stateFilePath in Directory.GetFiles(StatesPath))
        {
            var stateId = Path.GetFileName(stateFilePath).TrimEnd(".state");
            var stateValue = await File.ReadAllTextAsync(stateFilePath);
            states.Add(stateId, stateValue);
        }
        return states;
    }
}