using SimpleMockServer.Common.Extensions;
using SimpleMockServer.Domain.Configuration.Rules.ValuePatternParsing;
using SimpleMockServer.Domain.Generating;
using SimpleMockServer.Domain.TextPart.Variables.Global;

namespace SimpleMockServer.Domain.Configuration.Rules;

internal class GlobalVariablesParser
{
    private readonly IGlobalVariableSet _variableSet;
    private readonly ITextPartsParser _textPartsParser;

    public GlobalVariablesParser(IGlobalVariableSet variableSet, ITextPartsParser textPartsParser)
    {
        _variableSet = variableSet;
        _textPartsParser = textPartsParser;
    }

    public async Task Load(string path)
    {
        var variablesFiles = Directory.GetFiles(path, "*.global.var", SearchOption.AllDirectories);

        foreach (var variableFile in variablesFiles)
        {
            try
            {
                await LoadFromFile(variableFile);
            }
            catch(Exception e)
            {
                throw new Exception($"An error occured while parse file: {variableFile}", e);
            }
        }
    }

    private async Task LoadFromFile(string variableFile)
    {
        var lines = await File.ReadAllLinesAsync(variableFile);
        foreach (var line in lines)
        {
            string cleanLine = line.Trim();
            if (cleanLine.StartsWith(Constants.ControlChars.Comment))
                continue;

            if (string.IsNullOrEmpty(cleanLine))
                continue;

            _variableSet.Add(ParseLine(cleanLine));
        }
    }

    private GlobalVariable ParseLine(string line)
    {
        (string name, string pattern) = line.SplitToTwoPartsRequired(Constants.ControlChars.AssignmentOperator).Trim();
        
        var parts = _textPartsParser.Parse(pattern, _variableSet);

        var notAccessibleParts = parts.Where(p => p is not IGlobalTextPart);
        if(notAccessibleParts.Any())
        {
            string partsNames = string.Join(", ", notAccessibleParts.Select(x => x.GetType().FullName));

            throw new Exception($"{partsNames} cannot be use in global variables because they require http request");
        }

        return new GlobalVariable(name, parts.Cast<IGlobalTextPart>().ToArray());
    }
}
