using System.Text.Json;
using Lira.Domain.Configuration.RangeModel.Dto;
using Lira.Domain.DataModel;

namespace Lira.Domain.Configuration.RangeModel;

class RangesLoader
{
    private readonly GuidParser _guidParser;
    private readonly IntParser _intParser;
    private readonly DecParser _decParser;
    private readonly HexParser _hexParser;

    public RangesLoader(GuidParser guidParser, IntParser intParser, DecParser decParser, HexParser hexParser)
    {
        _guidParser = guidParser;
        _intParser = intParser;
        _decParser = decParser;
        _hexParser = hexParser;
    }

    public async Task<Dictionary<DataName, Data>> Load(string path)
    {
        var dataFiles = DirectoryHelper.GetFiles(path, "*.ranges.json");
        var dataWithRefs = new List<Data>();

        foreach (var dataFile in dataFiles)
        {
            try
            {
                var json = await File.ReadAllTextAsync(dataFile);
                dataWithRefs.AddRange(CreateRanges(json));
            }
            catch (Exception exc)
            {
                throw new Exception($"An error has occurred while parse file: '{dataFile}'", exc);
            }
        }

        return dataWithRefs.ToDictionary(x => x.Name, x => x);
    }

    private IReadOnlyCollection<Data> CreateRanges(string json)
    {
        Dictionary<string, DataOptionsDto> root;
        try
        {
            root = JsonSerializer.Deserialize<Dictionary<string, DataOptionsDto>>(json)!;
        }
        catch (Exception exc)
        {
            throw new Exception($"An error has occurred on deserialize json: '{json}'", exc);
        }

        var result = new List<Data>();

        foreach (var data in root)
        {
            var name = new DataName(data.Key);
            if (data.Value.Ranges.Length == 0)
                throw new Exception($"Data '{name}' not contains ranges");

            try
            {
                result.Add(CreateData(name, data.Value));
            }
            catch (Exception e)
            {
                throw new Exception($"An error has occurred on parse data '{data.Key}'", e);
            }
        }

        return result;
    }

    private Data CreateData(DataName name, DataOptionsDto dataOptionsDto)
    {
        switch (dataOptionsDto.Type)
        {
            case "guid":
                return _guidParser.Parse(name, dataOptionsDto);
            case "int":
                return _intParser.Parse(name, dataOptionsDto);
            case "dec":
                return _decParser.Parse(name, dataOptionsDto);
            case "hex":
                return _hexParser.Parse(name, dataOptionsDto);
            default:
                throw new Exception("Type not defined for data. Known types: guid, int, dec");
        }
    }
}