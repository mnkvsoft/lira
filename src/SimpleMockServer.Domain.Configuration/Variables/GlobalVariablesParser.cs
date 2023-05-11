using SimpleMockServer.Common;
using SimpleMockServer.Domain.Configuration.Rules.ValuePatternParsing;
using SimpleMockServer.Domain.TextPart;
using SimpleMockServer.Domain.TextPart.Variables;
using SimpleMockServer.FileSectionFormat;

namespace SimpleMockServer.Domain.Configuration.Variables;

internal class GlobalVariablesParser
{
    private readonly VariablesParser _variablesParser;

    public GlobalVariablesParser(VariablesParser variablesParser)
    {
        _variablesParser = variablesParser;
    }

    public async Task<IReadOnlyCollection<Variable>> Load(ParsingContext parsingContext, string path)
    {
        var result = new List<Variable>();

        result.AddRange(await GetVariablesFromFiles(
            parsingContext,
            Directory.GetFiles(path, "*.global.var", SearchOption.AllDirectories),
            CreateGlobalVariable));

        result.AddRange(await GetVariablesFromFiles(
            parsingContext with { Variables = parsingContext.Variables.Combine(result)},
            Directory.GetFiles(path, "*.req.var", SearchOption.AllDirectories),
            (name, parts) => new RequestVariable(name, parts)));

        return result;
    }

    private async Task<VariableSet> GetVariablesFromFiles(ParsingContext parsingContext, string[] variablesFiles, Func<string, ObjectTextParts, Variable> createVariable)
    {
        var result = new VariableSet();
        foreach (var variableFile in variablesFiles)
        {
            try
            {
                var lines = TextCleaner.DeleteEmptiesAndComments(await File.ReadAllTextAsync(variableFile));
                result.AddRange(await _variablesParser.Parse(lines, parsingContext with { CurrentPath = variableFile.GetDirectory()}, createVariable));
            }
            catch (Exception exc)
            {
                throw new FileParsingException(variableFile, exc);
            }
        }
        return result;
    }

    private Variable CreateGlobalVariable(string name, ObjectTextParts parts)
    {
        var notAccessibleParts = parts.Where(p => p is not IGlobalObjectTextPart).ToArray();
        
        if (notAccessibleParts.Any())
        {
            var partsNames = string.Join(", ", notAccessibleParts.Select(x => x.GetType().FullName));
            throw new Exception($"{partsNames} cannot be use in global variables because they require http request");
        }

        return new GlobalObjectVariable(name, parts.Cast<IGlobalObjectTextPart>().ToArray());
    }
}
