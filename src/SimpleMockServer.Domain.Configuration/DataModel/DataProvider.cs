using System.Text.Json;
using SimpleMockServer.Domain.Configuration.DataModel.Dto;
using SimpleMockServer.Domain.DataModel;

namespace SimpleMockServer.Domain.Configuration.DataModel;

class DataLoader
{
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
                throw new Exception($"An error has occurred while parse file: '{dataFile}'", exc);
            }
        }

        return dataWithRefs.ToDictionary(x => x.Name, x => x);
    }

    private IReadOnlyCollection<Data> CreateDatas(string json)
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
                throw new Exception($"An error has accured on parse data '{data.Key}'", e);
            }
        }

        return result;
    }

    private static Data CreateData(DataName name, DataOptionsDto dataOptionsDto)
    {
        switch (dataOptionsDto.Type)
        {
            case "guid":
                return GuidParser.Parse(name, dataOptionsDto);
            case "int":
                return IntParser.Parse(name, dataOptionsDto);
            case "float":
                return FloatParser.Parse(name, dataOptionsDto);
            default:
                throw new Exception("Type not defined for data. Known types: guid, int, float");
        }
    }
}