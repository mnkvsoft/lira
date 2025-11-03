using Lira.Domain.DataModel;

namespace Lira.Domain.Configuration;

class RangesProvider : IRangesProvider
{
    internal Dictionary<DataName, Data> Ranges
    {
        get
        {
            if (_ranges == null)
                throw new InvalidOperationException("Ranges not loaded yet");
            return _ranges;
        }
        set
        {
            _ranges = value;
        }
    }

    private Dictionary<DataName, Data>? _ranges = null;

    Data IRangesProvider.Get(DataName name)
    {
        if (!Ranges.TryGetValue(name, out var result))
            throw new Exception($"Interval '{name}' not found");

        return result;
    }

    public Data? Find(DataName name)
    {
        Ranges.TryGetValue(name, out var data);
        return data;
    }

    public IReadOnlyCollection<Data> GetAll()
    {
        return Ranges.Values;
    }
}