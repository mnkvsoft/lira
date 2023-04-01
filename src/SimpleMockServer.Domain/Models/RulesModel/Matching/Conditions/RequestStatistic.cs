using System.Collections;

namespace SimpleMockServer.Domain.Models.RulesModel.Matching.Conditions;

public record RequestStatisticEntry(DateTime InvokeTime);

public class RequestStatistic
{
    private readonly Dictionary<Guid, RequestStatisticEntry> _requestIdToStatisticEntryMap = new();
    public IReadOnlyCollection<RequestStatisticEntry> Entries => _requestIdToStatisticEntryMap.Values;

    public void AddIfNotExist(Guid requestId)
    {
        if (_requestIdToStatisticEntryMap.ContainsKey(requestId))
            return;

        _requestIdToStatisticEntryMap.Add(requestId, new RequestStatisticEntry(DateTime.Now));
    }
}

