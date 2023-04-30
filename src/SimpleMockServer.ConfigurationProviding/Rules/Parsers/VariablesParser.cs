using SimpleMockServer.Common.Extensions;
using SimpleMockServer.ConfigurationProviding.Rules.ValuePatternParsing;
using SimpleMockServer.Domain.Models.RulesModel.Generating;
using SimpleMockServer.FileSectionFormat;

namespace SimpleMockServer.ConfigurationProviding.Rules.Parsers;

class VariablesParser
{
    private readonly ITextPartsParser _textGeneratorFactory;

    public VariablesParser(ITextPartsParser textGeneratorFactory)
    {
        _textGeneratorFactory = textGeneratorFactory;
    }

    internal VariableSet Parse(FileSection variablesSection)
    {
        VariableSet set = new VariableSet();

        foreach(var line in variablesSection.LinesWithoutBlock)
        {
            (string name, string? pattern) = line.SplitToTwoParts("=").Trim();

            if(string.IsNullOrEmpty(name))
                throw new Exception($"Variable name not defined. Line: {line}");

            if (string.IsNullOrEmpty(pattern))
                throw new Exception($"Variable '{name}' not initialized. Line: {line}");

            var generator = _textGeneratorFactory.Parse(pattern, set);
            set.Add(new TextPart.Variable(name, generator));
        }

        return set;
    }
}
