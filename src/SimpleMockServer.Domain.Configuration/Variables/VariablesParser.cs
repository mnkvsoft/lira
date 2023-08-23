using System.Text;
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

    public async Task<VariableSet> Parse(IReadOnlyCollection<string> lines, ParsingContext parsingContext,
        Func<string, ObjectTextParts, Variable> create)
    {
        var all = new VariableSet(parsingContext.Variables);
        var newContext = parsingContext with { Variables = all };

        var onlyNew = new VariableSet();


        string? variableName = null;
        var variablePattern = new StringBuilder();

        bool isFirstLine = true;
        foreach (var line in lines)
        {
            if (IsDefineVariable(line))
            {
                if (!isFirstLine)
                {
                    await AddVariable(variableName, variablePattern);
                    variablePattern.Clear();
                }

                var (name, pattern) = line.SplitToTwoParts(Consts.ControlChars.AssignmentOperator).Trim();
                variableName = name.TrimStart(Consts.ControlChars.VariablePrefix);
                variablePattern.Append(pattern);
            }
            else
            {
                variablePattern.Append((variablePattern.Length > 0 ? Environment.NewLine : "") + line);
            }

            isFirstLine = false;
        }

        await AddVariable(variableName, variablePattern);
        
        return onlyNew;
        
        async Task AddVariable(string? s, StringBuilder stringBuilder)
        {
            if (string.IsNullOrEmpty(s))
                throw new Exception("Variable name not defined");

            string p = stringBuilder.ToString();
            if (string.IsNullOrEmpty(p))
                throw new Exception($"Variable '{s}' not initialized");

            var parts = await _textPartsParser.Parse(p, newContext);

            all.Add(create(s, parts));
            onlyNew.Add(create(s, parts));
        }
    }

    private static bool IsDefineVariable(string line)
    {
        if (line.StartsWith(Consts.ControlChars.VariablePrefix))
        {
            var idx = line.IndexOf(' ');
            if (idx is -1 or 1)
                return false;

            var afterName = line[idx..];
            return afterName.TrimStart(' ').StartsWith(Consts.ControlChars.AssignmentOperator);
        }

        return false;
    }
}