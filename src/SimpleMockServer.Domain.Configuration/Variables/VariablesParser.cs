using SimpleMockServer.Domain.Configuration.Rules.ValuePatternParsing;
using SimpleMockServer.Domain.TextPart;
using SimpleMockServer.Domain.TextPart.Custom.Variables;

namespace SimpleMockServer.Domain.Configuration.Variables;

class VariablesParser
{
    private readonly ITextPartsParser _textPartsParser;

    public VariablesParser(ITextPartsParser textPartsParser)
    {
        _textPartsParser = textPartsParser;
    }

    public async Task<VariableSet> Parse(IReadOnlyCollection<string> lines, ParsingContext parsingContext,
        Func<string, ObjectTextParts, Variable> create)
    {
        var all = new VariableSet(parsingContext.Variables);
        var newContext = parsingContext with { Variables = all };

        var onlyNew = new VariableSet();

        foreach (var (name, pattern) in VariableAndTemplatesUtils.GetNameToPatternMap(lines, Consts.ControlChars.VariablePrefix))
        {
            ObjectTextParts parts = await _textPartsParser.Parse(pattern, newContext);

            all.Add(create(name, parts));
            onlyNew.Add(create(name, parts));    
        }
        
        return onlyNew;
    }

    
}