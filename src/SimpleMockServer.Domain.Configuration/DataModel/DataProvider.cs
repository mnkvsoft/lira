using System.Text.Json;
using System.Text.Json.Serialization;
using SimpleMockServer.Common;
using SimpleMockServer.Domain.DataModel;
using SimpleMockServer.Domain.DataModel.DataImpls.Guid;
using SimpleMockServer.Domain.DataModel.DataImpls.Number;
using SimpleMockServer.Domain.DataModel.DataImpls.Number.Ranges;

namespace SimpleMockServer.Domain.Configuration.DataModel;

class DataLoader
{
    private const string DefaultSeqName = "default_system";

    public async Task<Dictionary<DataName, Data>> Load(string path)
    {
        var dataFiles = DirectoryHelper.GetFiles(path, "*.data.json");
        var dataWithRefs = new List<Data>();

        foreach (var dataFile in dataFiles)
        {
            try
            {
                var json = await File.ReadAllTextAsync(dataFile);
                dataWithRefs.AddRange(CreateDatas(json));
            }
            catch (Exception exc)
            {
                throw new Exception(dataFile, exc);
            }
        }

        return dataWithRefs.ToDictionary(x => x.Name, x => x);
    }

    private IReadOnlyCollection<Data> CreateDatas(string json)
    {
        DataRootDto root;
        try
        {
            root = JsonSerializer.Deserialize<DataRootDto>(json)!;
        }
        catch (Exception exc)
        {
            throw new Exception($"An error has acсured on deserialize json: '{json}'", exc);
        }

        var result = new List<Data>();

        foreach (var data in root.Data)
        {
            var name = new DataName(data.Key);

            try
            {
                result.Add(CreateData(name, data.Value));
            }
            catch (Exception e)
            {
                throw new Exception($"An error has acсured on parse data '{data.Key}'", e);
            }
        }

        return result;
    }

    private static Data CreateData(DataName name, TypeDto dataType)
    {
        if (dataType.Guid != null)
        {
            return CreateGuidData(name, dataType.Guid);
        }
        else if (dataType.Int != null)
        {
            return CreateNumberData(name, dataType.Int);
        }
        else
        {
            throw new Exception("Type not defined for data. Known types: guid, number");
        }
    }

    private static NumberData CreateNumberData(DataName name, DataTypeDto number)
    {
        if (number.Seq != null)
        {
            return new NumberData(name, CreateNumberSeqDataRanges(number.Seq.Ranges));

        }
        else if (number.Set != null)
        {
            return new NumberData(name, CreateNumberSetDataRanges(number.Set.Ranges));
        }
        else
        {
            throw new Exception($"An error occurred while creating '{name}' data. For number access only 'seq' or 'set' values providing type");
        }
    }

    private static GuidData CreateGuidData(DataName name, DataTypeDto guid)
    {
        if (guid.Seq != null)
        {
            var numberSeqRanges = CreateNumberSeqDataRanges(guid.Seq.Ranges);
            return new GuidData(name, numberSeqRanges.ToGuidRangesDictionary());
        }
        else if (guid.Set != null)
        {
            var numberSetRanges = CreateNumberSetDataRanges(guid.Set.Ranges);
            return new GuidData(name, numberSetRanges.ToGuidRangesDictionary());
        }
        else
        {
            throw new Exception($"An error occurred while creating '{name}' data. For guid access only 'seq' or 'set' values providing type");
        }
    }

    private static IReadOnlyDictionary<DataName, NumberDataRange> CreateNumberSeqDataRanges(IReadOnlyDictionary<string, string> rangesDto)
    {
        var builder = new NumberSeqRangesBuilder();

        foreach (var range in rangesDto)
        {
            var rangeName = new DataName(range.Key);
            var value = range.Value;

            if (!TryParse(value, out var startInterval))
                throw new Exception($"An error occurred while creating range '{rangeName}'. Item '{value}' has not Int64 value");

            builder.Add(rangeName, startInterval);
        }

        Dictionary<DataName, Int64Interval> intervals;

        if (builder.Count == 0)
        {
            intervals = new Dictionary<DataName, Int64Interval>();
            intervals.Add(new DataName(DefaultSeqName), new Int64Interval(1, long.MaxValue));
        }
        else
        {
            intervals = builder.BuildIntervals();
        }

        var ranges = intervals.ToDictionary(x => x.Key, x => (NumberDataRange)new NumberSeqDataRange(x.Key, new Int64Sequence(x.Value)));
        return ranges;
    }

    private static IReadOnlyDictionary<DataName, NumberDataRange> CreateNumberSetDataRanges(Dictionary<string, string> rangesDto)
    {
        var ranges = new List<NumberDataRange>();
        foreach (var range in rangesDto)
        {
            var rangeName = new DataName(range.Key);

            var rangeValueRaw = range.Value;
            if (rangeValueRaw.Contains('-'))
            {
                var splitted = rangeValueRaw.Split('-');

                if (splitted.Length > 2)
                    throw new Exception($"An error occurred while creating '{rangeName}' range. Invalid interval: '{rangeValueRaw}'");

                if (!TryParse(splitted[0], out var from))
                    throw new Exception($"An error occurred while creating '{rangeName}' range. Invalid start interval: '{splitted[0]}'");

                if (!TryParse(splitted[1], out var to))
                    throw new Exception($"An error occurred while creating '{rangeName}' range. Invalid end interval: '{splitted[1]}'");

                ranges.Add(new NumberSetIntervalDataRange(rangeName, new Int64Interval(from, to)));
            }
            else
            {
                var splitted = rangeValueRaw.Split(',');
                var values = new List<long>();

                foreach (var strValue in splitted)
                {
                    if (!TryParse(strValue, out var val))
                        throw new Exception($"An error occurred while creating '{rangeName}' range. Invalid value: '{strValue}'");

                    values.Add(val);
                }

                ranges.Add(new NumberSetValuesDataRange(rangeName, values));
            }
        }

        AssertNotIntersect(ranges);

        return ranges.ToDictionary(x => x.Name, x => x);
    }

    private static void AssertNotIntersect(IReadOnlyList<NumberDataRange> ranges)
    {
        for (var i = 0; i < ranges.Count - 1; i++)
        {
            var curRange = ranges[i];
            var nextRange = ranges[i + 1];

            if (curRange is NumberSetIntervalDataRange curIntervalRange)
            {
                if (nextRange is NumberSetIntervalDataRange nextIntervalRange)
                {
                    AssertNotIntersect(curIntervalRange, nextIntervalRange);

                }
                else if (nextRange is NumberSetValuesDataRange nextValuesRange)
                {
                    AssertNotIntersect(curIntervalRange, nextValuesRange);
                }
                else
                {
                    throw new Exception($"Unknown type: " + curRange.GetType());
                }
            }
            else if (curRange is NumberSetValuesDataRange curValuesRange)
            {
                if (nextRange is NumberSetValuesDataRange nextValuesRange)
                {
                    AssertNotIntersect(curValuesRange, nextValuesRange);
                }
                else if (nextRange is NumberSetIntervalDataRange nextIntervalRange)
                {
                    AssertNotIntersect(nextIntervalRange, curValuesRange);
                }
                else
                {
                    throw new Exception($"Unknown type: " + curRange.GetType());
                }
            }
        }
    }

    private static void AssertNotIntersect(NumberSetValuesDataRange curValuesRange, NumberSetValuesDataRange nextValuesRange)
    {
        var intersectValues = curValuesRange.Values.Intersect(nextValuesRange.Values).ToArray();
        if (intersectValues.Length > 0)
            throw new Exception($"Values {string.Join(", ", intersectValues)} from range '{curValuesRange.Name}' intersect with '{nextValuesRange.Name}'");
    }

    private static void AssertNotIntersect(NumberSetIntervalDataRange curIntervalRange, NumberSetValuesDataRange nextValuesRange)
    {
        foreach (var value in nextValuesRange.Values)
        {
            if (curIntervalRange.Interval.InRange(value))
                throw new Exception($"Value {value} from '{nextValuesRange.Name}' range contains in '{curIntervalRange.Name}' range");
        }
    }

    private static void AssertNotIntersect(NumberSetIntervalDataRange curIntervalRange, NumberSetIntervalDataRange nextIntervalRange)
    {
        if (curIntervalRange.Interval.IsIntersect(nextIntervalRange.Interval))
            throw new Exception($"Range '{curIntervalRange.Name}' intersect with '{nextIntervalRange.Name}'");
    }

    private static bool TryParse(string str, out long result)
    {
        return long.TryParse(str.Replace("_", ""), out result);
    }

    record DataRootDto([property: JsonPropertyName("data")] Dictionary<string, TypeDto> Data);

    record TypeDto([property: JsonPropertyName("int")] DataTypeDto? Int, [property: JsonPropertyName("guid")] DataTypeDto? Guid);

    record DataTypeDto([property: JsonPropertyName("seq")] RangesDto? Seq, [property: JsonPropertyName("set")] RangesDto? Set);

    record RangesDto([property: JsonPropertyName("ranges")] Dictionary<string, string> Ranges);
}
