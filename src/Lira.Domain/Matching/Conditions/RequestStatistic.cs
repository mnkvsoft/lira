using System.Collections.Concurrent;

namespace Lira.Domain.Matching.Conditions;

public record RequestStatisticEntry(DateTime InvokeTime);

public class RequestStatistic
{
    private readonly ConcurrentBag<RequestStatisticEntry> _requestIdToStatisticEntryMap = new();
    public ICollection<RequestStatisticEntry> Entries => _requestIdToStatisticEntryMap.ToList();

    public void Add()
    {
        _requestIdToStatisticEntryMap.Add(new RequestStatisticEntry(DateTime.Now));
    }
}

