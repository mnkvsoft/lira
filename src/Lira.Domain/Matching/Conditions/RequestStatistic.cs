using System.Collections.Concurrent;

namespace Lira.Domain.Matching.Conditions;

public record RequestStatisticEntry(DateTime InvokeTime);

public class RequestStatistic
{
    private readonly ConcurrentDictionary<Guid, RequestStatisticEntry> _requestIdToStatisticEntryMap = new();
    public ICollection<RequestStatisticEntry> Entries => _requestIdToStatisticEntryMap.Values;

    public void AddIfNotExist(Guid requestId)
    {
        if (_requestIdToStatisticEntryMap.ContainsKey(requestId))
            return;

        _requestIdToStatisticEntryMap.TryAdd(requestId, new RequestStatisticEntry(DateTime.Now));
    }
}

