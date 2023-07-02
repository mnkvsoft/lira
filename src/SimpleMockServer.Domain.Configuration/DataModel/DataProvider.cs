using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using SimpleMockServer.Common;
using SimpleMockServer.Common.Exceptions;
using SimpleMockServer.Domain.DataModel;
using SimpleMockServer.Domain.DataModel.DataImpls.Float;
using SimpleMockServer.Domain.DataModel.DataImpls.Float.Ranges;
using SimpleMockServer.Domain.DataModel.DataImpls.Guid;
using SimpleMockServer.Domain.DataModel.DataImpls.Int;
using SimpleMockServer.Domain.DataModel.DataImpls.Int.Ranges;

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

        if (dataType.Int != null)
        {
            return CreateIntData(name, dataType.Int);
        }
        
        if (dataType.Float != null)
        {
            return CreateFloatData(name, dataType.Float);
        }

        throw new Exception("Type not defined for data. Known types: guid, int, float");
    }

    private static IntData CreateIntData(DataName name, DataTypeDto number)
    {
        if (number.Seq != null)
        {
            return new IntData(name, CreateIntSeqDataRanges(number.Seq.Ranges).ToLongRangesDictionary());
        }

        if (number.Set != null)
        {
            return new IntData(name, CreateNumberSetDataRanges<long>(number.Set.Ranges,
                (datName, interval) => new IntSetIntervalDataRange(datName, new Int64Interval(interval)),
                (datName, values) => new IntSetValuesDataRange(datName, values)));
        }

        throw new Exception($"An error occurred while creating '{name}' data. For number access only 'seq' or 'set' values providing type");
    }

    

    private static FloatData CreateFloatData(DataName name, FloatDataTypeDto number)
    {
        if (number.Set != null)
        {
            var numberSetDataRanges = CreateNumberSetDataRanges<float>(number.Set.Ranges,
                (datName, interval) => new FloatSetIntervalDataRange(datName, interval),
                (datName, values) => new FloatSetValuesDataRange(datName, values));
            return new FloatData(name, numberSetDataRanges);
        }

        throw new Exception($"An error occurred while creating '{name}' data. For number access only 'seq' or 'set' values providing type");
    }
    
    private static GuidData CreateGuidData(DataName name, DataTypeDto guid)
    {
        if (guid.Seq != null)
        {
            var intSeqRanges = CreateIntSeqDataRanges(guid.Seq.Ranges);
            return new GuidData(name, intSeqRanges.ToLongRangesDictionary().ToGuidRangesDictionary());
        }

        if (guid.Set != null)
        {
            var numberSetRanges = CreateNumberSetDataRanges<long>(
                guid.Set.Ranges, 
                (datName, interval) => new IntSetIntervalDataRange(datName, new Int64Interval(interval)),
                (datName, values) => new IntSetValuesDataRange(datName, values));
            
            return new GuidData(name, numberSetRanges.ToGuidRangesDictionary());
        }

        throw new Exception($"An error occurred while creating '{name}' data. For guid access only 'seq' or 'set' values providing type");
    }

    
    private static IReadOnlyDictionary<DataName, IntDataRange> CreateIntSeqDataRanges(IReadOnlyDictionary<string, string> rangesDto)
    {
        var builder = new IntSeqRangesBuilder();

        foreach (var range in rangesDto)
        {
            var rangeName = new DataName(range.Key);
            var value = range.Value;

            if (!TryParseInt(value, out var startInterval))
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

        var ranges = intervals.ToDictionary(x => x.Key, x => (IntDataRange)new IntSeqDataRange(x.Key, new Int64Sequence(x.Value)));
        return ranges;
    }

    private static IReadOnlyDictionary<DataName, DataRange<TNumber>> CreateNumberSetDataRanges<TNumber>(
        Dictionary<string, string> rangesDto,
        Func<DataName, Interval<TNumber>, DataRange<TNumber>> createInterval,
        Func<DataName, IReadOnlyList<TNumber>, DataRange<TNumber>> createValues) 
        where TNumber : struct, IComparable<TNumber>
    {
        var ranges = new List<DataRange<TNumber>>();
        foreach (var range in rangesDto)
        {
            var rangeName = new DataName(range.Key);

            var rangeValueRaw = range.Value;
            
            if(Interval<TNumber>.TryParse(rangeValueRaw, out var interval))
            {            
                ranges.Add(createInterval(rangeName, interval));
                continue;
            }

            var splitted = rangeValueRaw.Trim().TrimStart('[').TrimEnd(']').Split(',');
            var values = new List<TNumber>();

            foreach (var strValue in splitted)
            {
                var converter = TypeDescriptor.GetConverter(typeof(TNumber));
                object? objValue = converter.ConvertFromInvariantString(strValue);
                        
                if (objValue == null)
                    throw new Exception($"An error occurred while creating '{rangeName}' range. Invalid value: '{strValue}'");

                values.Add((TNumber)objValue);
            }

            ranges.Add(createValues(rangeName, values));
        }

        AssertNotIntersect(ranges);

        return ranges.ToDictionary(x => x.Name, x => x);
    }

    private static void AssertNotIntersect<TNumber>(IReadOnlyList<DataRange<TNumber>> ranges) where TNumber : struct
    {
        for (var i = 0; i < ranges.Count - 1; i++)
        {
            var curRange = ranges[i];
            var nextRange = ranges[i + 1];

            if (curRange is IntSetIntervalDataRange curIntervalRange)
            {
                if (nextRange is IntSetIntervalDataRange nextIntervalRange)
                {
                    AssertNotIntersect(curIntervalRange, nextIntervalRange);

                }
                else if (nextRange is IntSetValuesDataRange nextValuesRange)
                {
                    AssertNotIntersect(curIntervalRange, nextValuesRange);
                }
                else
                {
                    throw new UnsupportedInstanceType(curRange);
                }
            }
            else if (curRange is IntSetValuesDataRange curValuesRange)
            {
                if (nextRange is IntSetValuesDataRange nextValuesRange)
                {
                    AssertNotIntersect(curValuesRange, nextValuesRange);
                }
                else if (nextRange is IntSetIntervalDataRange nextIntervalRange)
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

    private static void AssertNotIntersect(IntSetValuesDataRange curValuesRange, IntSetValuesDataRange nextValuesRange)
    {
        var intersectValues = curValuesRange.Values.Intersect(nextValuesRange.Values).ToArray();
        if (intersectValues.Length > 0)
            throw new Exception($"Values {string.Join(", ", intersectValues)} from range '{curValuesRange.Name}' intersect with '{nextValuesRange.Name}'");
    }

    private static void AssertNotIntersect(IntSetIntervalDataRange curIntervalRange, IntSetValuesDataRange nextValuesRange)
    {
        foreach (var value in nextValuesRange.Values)
        {
            if (curIntervalRange.Interval.InRange(value))
                throw new Exception($"Value {value} from '{nextValuesRange.Name}' range contains in '{curIntervalRange.Name}' range");
        }
    }

    private static void AssertNotIntersect(IntSetIntervalDataRange curIntervalRange, IntSetIntervalDataRange nextIntervalRange)
    {
        if (curIntervalRange.Interval.IsIntersect(nextIntervalRange.Interval))
            throw new Exception($"Range '{curIntervalRange.Name}' intersect with '{nextIntervalRange.Name}'");
    }

    private static bool TryParseInt(string str, out long result)
    {
        return long.TryParse(str.Replace("_", ""), out result);
    }

    record DataRootDto([property: JsonPropertyName("data")] Dictionary<string, TypeDto> Data);

    record TypeDto(
        [property: JsonPropertyName("int")] DataTypeDto? Int, 
        [property: JsonPropertyName("float")] FloatDataTypeDto? Float, 
        [property: JsonPropertyName("guid")] DataTypeDto? Guid);

    record DataTypeDto(
        [property: JsonPropertyName("seq")] RangesDto? Seq, 
        [property: JsonPropertyName("set")] RangesDto? Set);

    record FloatDataTypeDto([property: JsonPropertyName("set")] RangesDto? Set);
    
    record RangesDto([property: JsonPropertyName("ranges")] Dictionary<string, string> Ranges);
}
