using System.Collections.Immutable;

namespace Lira.Domain.Configuration;

class StateRepository
{
    private static readonly string StatesPath = Path.Combine(Path.GetTempPath(), "lira", "states");

    public async Task UpdateStates(IReadOnlyDictionary<string, string> states)
    {
        if(!Directory.Exists(StatesPath))
            Directory.CreateDirectory(StatesPath);

        foreach (var (stateId, stateValue) in states)
        {
            var path = Path.Combine(StatesPath, stateId);
            await File.WriteAllTextAsync(path, stateValue);
        }
    }

    public async Task<IReadOnlyDictionary<string, string>> GetStates()
    {
        if(!Directory.Exists(StatesPath))
            return ImmutableDictionary<string, string>.Empty;

        var states = new Dictionary<string, string>();
        foreach (var stateFilePath in Directory.GetFiles(StatesPath))
        {
            var stateId = Path.GetFileName(stateFilePath);
            var stateValue = await File.ReadAllTextAsync(stateFilePath);
            states.Add(stateId, stateValue);
        }
        return states;
    }
}