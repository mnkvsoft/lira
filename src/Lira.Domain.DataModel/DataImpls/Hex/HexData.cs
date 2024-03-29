namespace Lira.Domain.DataModel.DataImpls.Hex;

public class HexData : Data<string>
{
    public HexData(DataName name, IReadOnlyDictionary<DataName, HexDataRange> ranges, string info)
        : base(name, ranges.ToDictionary(p => p.Key, p => (DataRange<string>)p.Value), info)
    { 
    }
}
