using SimpleMockServer.Common;
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

    public async Task<IReadOnlyCollection<Variable>> Load(ParsingContext parsingContext, string path)
    {
        var result = new VariableSet();
        
        await AddVariablesFromFiles(
            parsingContext,
            result,
            Directory.GetFiles(path, "*.global.var", SearchOption.AllDirectories),
            CreateGlobalVariable);

        await AddVariablesFromFiles(
            parsingContext,
            result,
            Directory.GetFiles(path, "*.req.var", SearchOption.AllDirectories),
            CreateRequestVariable);

        return result;
    }

    private async Task AddVariablesFromFiles(ParsingContext parsingContext, VariableSet set, string[] variablesFiles, Func<string, ParsingContext, Task<Variable>> createVariable)
    {
        foreach (var variableFile in variablesFiles)
        {
            try
            {
                var lines = CleanLines(await File.ReadAllLinesAsync(variableFile));

                foreach (var line in lines)
                {
                    set.Add(await createVariable(line, parsingContext with
                    {
                        Variables = set, 
                        CurrentPath = variableFile.GetDirectory()
                    }));
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

    private async Task<Variable> CreateGlobalVariable(string line, ParsingContext parsingContext)
    {
        var (name, pattern) = line.SplitToTwoPartsRequired(Constants.ControlChars.AssignmentOperator).Trim();

        var parts = await _textPartsParser.Parse(pattern, parsingContext);

        var notAccessibleParts = parts.Where(p => p is not IGlobalObjectTextPart).ToArray();
        
        if (notAccessibleParts.Any())
        {
            var partsNames = string.Join(", ", notAccessibleParts.Select(x => x.GetType().FullName));
            throw new Exception($"{partsNames} cannot be use in global variables because they require http request");
        }

        return new GlobalObjectVariable(name, parts.Cast<IGlobalObjectTextPart>().ToArray());
    }

    private async Task<Variable> CreateRequestVariable(string line, ParsingContext parsingContext)
    {
        var (name, pattern) = line.SplitToTwoPartsRequired(Constants.ControlChars.AssignmentOperator).Trim();

        var parts = await _textPartsParser.Parse(pattern, parsingContext);
        return new RequestVariable(name, parts);
    }
}
