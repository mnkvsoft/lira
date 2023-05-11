using SimpleMockServer.Common.Extensions;
using SimpleMockServer.Domain.Configuration.Rules.ValuePatternParsing;
using SimpleMockServer.Domain.TextPart;
using SimpleMockServer.Domain.TextPart.Variables;

namespace SimpleMockServer.Domain.Configuration.Variables;

class VariablesParser
{
    private readonly ITextPartsParser _textPartsParser;

    public VariablesParser(ITextPartsParser textPartsParser)
    {
        _textPartsParser = textPartsParser;
    }

    public async Task<VariableSet> Parse(IReadOnlyCollection<string> lines, ParsingContext parsingContext, Func<string, ObjectTextParts, Variable> create)
    {
        var all = new VariableSet(parsingContext.Variables);
        var newContext = parsingContext with { Variables = all };
        
        var onlyNew = new VariableSet();

        foreach (var line in lines)
        {
            var (name, pattern) = line.SplitToTwoParts("=").Trim();

            if (string.IsNullOrEmpty(name))
                throw new Exception($"Variable name not defined. Line: {line}");

            if (string.IsNullOrEmpty(pattern))
                throw new Exception($"Variable '{name}' not initialized. Line: {line}");

            var parts = await _textPartsParser.Parse(pattern, newContext );
            
            all.Add(create(name, parts));
            onlyNew.Add(create(name, parts));
        }

        return onlyNew;
    }
}
