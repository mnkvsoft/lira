using SimpleMockServer.Common.Extensions;
using SimpleMockServer.Domain.Configuration.Rules.ValuePatternParsing;
using SimpleMockServer.Domain.TextPart;
using SimpleMockServer.Domain.TextPart.Variables;

namespace SimpleMockServer.Domain.Configuration.Rules.Parsers.Variables;

internal class GlobalVariablesParser
{
    
    private readonly ITextPartsParser _textPartsParser;

    public GlobalVariablesParser(ITextPartsParser textPartsParser)
    {
        _textPartsParser = textPartsParser;
    }

    public async Task<IReadOnlyCollection<Variable>> Load(string path)
    {
        var result = new VariableSet();
        
        await AddVariablesFromFiles(
            result,
            Directory.GetFiles(path, "*.global.var", SearchOption.AllDirectories),
            CreateGlobalVariable);

        await AddVariablesFromFiles(
            result,
            Directory.GetFiles(path, "*.req.var", SearchOption.AllDirectories),
            CreateRequestVariable);

        return result;
    }

    private async Task AddVariablesFromFiles(VariableSet registeredVariables, string[] variablesFiles, Func<string, VariableSet, Variable> createVariable)
    {
        foreach (var variableFile in variablesFiles)
        {
            try
            {
                var lines = CleanLines(await File.ReadAllLinesAsync(variableFile));

                foreach (var line in lines)
                {
                    registeredVariables.Add(createVariable(line, registeredVariables));
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

    private GlobalObjectVariable CreateGlobalVariable(string line, VariableSet registeredVariables)
    {
        var (name, pattern) = line.SplitToTwoPartsRequired(Constants.ControlChars.AssignmentOperator).Trim();

        var parts = _textPartsParser.Parse(pattern, registeredVariables);

        var notAccessibleParts = parts.Where(p => p is not IGlobalObjectTextPart).ToArray();
        
        if (notAccessibleParts.Any())
        {
            var partsNames = string.Join(", ", notAccessibleParts.Select(x => x.GetType().FullName));
            throw new Exception($"{partsNames} cannot be use in global variables because they require http request");
        }

        return new GlobalObjectVariable(name, parts.Cast<IGlobalObjectTextPart>().ToArray());
    }

    private RequestVariable CreateRequestVariable(string line, VariableSet registeredVariables)
    {
        var (name, pattern) = line.SplitToTwoPartsRequired(Constants.ControlChars.AssignmentOperator).Trim();

        var parts = _textPartsParser.Parse(pattern, registeredVariables);
        return new RequestVariable(name, parts);
    }
}
