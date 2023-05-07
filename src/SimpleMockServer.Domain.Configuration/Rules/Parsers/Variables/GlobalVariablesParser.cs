using SimpleMockServer.Common.Extensions;
using SimpleMockServer.Domain.Configuration.Rules.ValuePatternParsing;
using SimpleMockServer.Domain.TextPart.Variables;

namespace SimpleMockServer.Domain.Configuration.Rules.Parsers.Variables;

internal class GlobalVariablesParser
{
    private readonly GlobalVariableSet _variableSet;
    private readonly ITextPartsParser _textPartsParser;

    public GlobalVariablesParser(GlobalVariableSet variableSet, ITextPartsParser textPartsParser)
    {
        _variableSet = variableSet;
        _textPartsParser = textPartsParser;
    }

    public async Task Load(string path)
    {
        await AddVariablesFromFiles(
            Directory.GetFiles(path, "*.global.var", SearchOption.AllDirectories),
            CreateGlobalVariable);

        await AddVariablesFromFiles(
            Directory.GetFiles(path, "*.req.var", SearchOption.AllDirectories),
            CreateRequestVariable);
    }

    private async Task AddVariablesFromFiles(string[] variablesFiles, Func<string, GlobalVariableSet, Variable> createVariable)
    {
        foreach (var variableFile in variablesFiles)
        {
            try
            {
                var lines = CleanLines(await File.ReadAllLinesAsync(variableFile));

                foreach (var line in lines)
                {
                    _variableSet.Add(createVariable(line, _variableSet));
                }
            }
            catch (Exception exc)
            {
                throw new FileParsingException(variableFile, exc);
            }
        }
    }

    private IReadOnlyCollection<string> CleanLines(IReadOnlyCollection<string> lines)
    {
        var result = new List<string>();
        foreach (var line in lines)
        {
            var cleanLine = line.Trim();
            if (cleanLine.StartsWith(Constants.ControlChars.Comment))
                continue;

            if (string.IsNullOrEmpty(cleanLine))
                continue;

            result.Add(cleanLine);
        }
        return result;
    }

    private GlobalVariable CreateGlobalVariable(string line, GlobalVariableSet registeredVariables)
    {
        var (name, pattern) = line.SplitToTwoPartsRequired(Constants.ControlChars.AssignmentOperator).Trim();

        var parts = _textPartsParser.Parse(pattern, registeredVariables);

        var notAccessibleParts = parts.Where(p => p is not IGlobalTextPart).ToArray();
        
        if (notAccessibleParts.Any())
        {
            var partsNames = string.Join(", ", notAccessibleParts.Select(x => x.GetType().FullName));
            throw new Exception($"{partsNames} cannot be use in global variables because they require http request");
        }

        return new GlobalVariable(name, parts.Cast<IGlobalTextPart>().ToArray());
    }

    private RequestVariable CreateRequestVariable(string line, GlobalVariableSet registeredVariables)
    {
        var (name, pattern) = line.SplitToTwoPartsRequired(Constants.ControlChars.AssignmentOperator).Trim();

        var parts = _textPartsParser.Parse(pattern, registeredVariables);
        return new RequestVariable(name, parts);
    }
}
