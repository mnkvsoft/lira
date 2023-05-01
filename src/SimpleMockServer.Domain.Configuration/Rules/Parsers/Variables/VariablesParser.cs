using SimpleMockServer.Common.Extensions;
using SimpleMockServer.Domain.Configuration.Rules.ValuePatternParsing;
using SimpleMockServer.Domain.TextPart.Variables;
using SimpleMockServer.FileSectionFormat;

namespace SimpleMockServer.Domain.Configuration.Rules.Parsers.Variables;

class VariablesParser
{
    private readonly ITextPartsParser _textGeneratorFactory;

    public VariablesParser(ITextPartsParser textGeneratorFactory)
    {
        _textGeneratorFactory = textGeneratorFactory;
    }

    public VariableSet Parse(FileSection variablesSection, IReadOnlyCollection<Variable> registeredVariables)
    {
        var set = new VariableSet(registeredVariables);

        foreach (var line in variablesSection.LinesWithoutBlock)
        {
            (var name, var pattern) = line.SplitToTwoParts("=").Trim();

            if (string.IsNullOrEmpty(name))
                throw new Exception($"RequestVariable name not defined. Line: {line}");

            if (string.IsNullOrEmpty(pattern))
                throw new Exception($"RequestVariable '{name}' not initialized. Line: {line}");

            var generator = _textGeneratorFactory.Parse(pattern, set);
            set.Add(new RequestVariable(name, generator));
        }

        return set;
    }
}
