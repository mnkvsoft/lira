using SimpleMockServer.Common.Extensions;
using SimpleMockServer.ConfigurationProviding.Rules.ValuePatternParsing;
using SimpleMockServer.Domain.Models.RulesModel.Generating;
using SimpleMockServer.FileSectionFormat;

namespace SimpleMockServer.ConfigurationProviding.Rules.Parsers;

class VariablesParser
{
    private readonly ValuePartsCreator _valuePartsCreator;

    public VariablesParser(ValuePartsCreator valuePartsCreator)
    {
        _valuePartsCreator = valuePartsCreator;
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

            var parts = _valuePartsCreator.Create(pattern, set);
            set.Add(new ValuePart.Variable(name, parts));
        }

        return set;
    }
}
