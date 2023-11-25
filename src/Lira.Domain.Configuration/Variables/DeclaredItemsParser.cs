﻿using System.Text;
using Lira.Common.Extensions;
using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.Domain.TextPart;
using Lira.Domain.TextPart.Impl.Custom;
using Lira.Domain.TextPart.Impl.Custom.FunctionModel;
using Lira.Domain.TextPart.Impl.Custom.VariableModel;

namespace Lira.Domain.Configuration.Variables;

class DeclaredItemsParser
{
    private readonly ITextPartsParser _textPartsParser;

    public DeclaredItemsParser(ITextPartsParser textPartsParser)
    {
        _textPartsParser = textPartsParser;
    }

    public async Task<IReadonlyDeclaredItems> Parse(IReadOnlyCollection<string> lines, ParsingContext parsingContext)
    {
        var all = new DeclaredItems(parsingContext.DeclaredItems);
        var newContext = parsingContext with { DeclaredItems = all };

        var onlyNew = new DeclaredItems();

        foreach (var (nameWithType, pattern) in GetNameToPatternMap(lines, Consts.ControlChars.VariablePrefix,
                     Consts.ControlChars.FunctionPrefix))
        {
            ObjectTextParts parts = await _textPartsParser.Parse(pattern, newContext);
            
            var (name, type) = nameWithType.SplitToTwoParts(":").Trim();
            if (type != null)
            {
                switch (type)
                {
                    case "json":
                        parts = new ObjectTextParts(new[] { new JsonWrapper(parts, name) });
                        break;
                    default:
                        throw new Exception($"Unknown type '{type}' for '{name}'");
                }
            }

            if (name.StartsWith(Consts.ControlChars.VariablePrefix))
            {
                var variable = new Variable(new CustomItemName(name.TrimStart(Consts.ControlChars.VariablePrefix)), parts);
                all.Variables.Add(variable);
                onlyNew.Variables.Add(variable);
            }
            else if (name.StartsWith(Consts.ControlChars.FunctionPrefix))
            {
                var function = new Function(new CustomItemName(name.TrimStart(Consts.ControlChars.FunctionPrefix)), parts);
                all.Functions.Add(function);
                onlyNew.Functions.Add(function);
            }
            else
            {
                throw new Exception($"Unknown declaration type: '{name}'");
            }
        }

        return onlyNew;
    }

    private static Dictionary<string, string> GetNameToPatternMap(IReadOnlyCollection<string> lines, params string[] namePrefixes)
    {
        var nameToValueMap = new Dictionary<string, string>();

        void AddVariable(string? s, StringBuilder stringBuilder)
        {
            if (string.IsNullOrEmpty(s))
                throw new Exception("Variable name not defined");

            string p = stringBuilder.ToString();
            if (string.IsNullOrEmpty(p))
                throw new Exception($"Variable '{s}' not initialized");

            nameToValueMap.Add(s, stringBuilder.ToString());
        }

        string? variableName = null;
        var variablePattern = new StringBuilder();

        bool isFirstLine = true;
        foreach (var line in lines)
        {
            if (IsDefineVariable(line, namePrefixes))
            {
                if (!isFirstLine)
                {
                    AddVariable(variableName, variablePattern);
                    variablePattern.Clear();
                }

                var (name, pattern) = line.SplitToTwoParts(Consts.ControlChars.AssignmentOperator).Trim();
                variableName = name;
                variablePattern.Append(pattern);
            }
            else
            {
                variablePattern.Append((variablePattern.Length > 0 ? Environment.NewLine : "") + line);
            }

            isFirstLine = false;
        }

        AddVariable(variableName, variablePattern);

        return nameToValueMap;
    }

    private static bool IsDefineVariable(string line, string[] namePrefixes)
    {
        if (namePrefixes.Any(line.StartsWith))
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