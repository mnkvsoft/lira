using System.Collections.Frozen;
using System.Text;
using Lira.Common;
using Lira.Common.Extensions;
using Lira.Domain.TextPart;
using Lira.Domain.TextPart.Impl.Custom.FunctionModel;
using Lira.Domain.TextPart.Impl.Custom.VariableModel.RuleVariables;

namespace Lira.Domain.Configuration.DeclarationItems;

class DeclaredItemsParser
{
    public IReadOnlySet<DeclaredItemDraft> Parse(IReadOnlyCollection<string> lines, string declarationSource)
    {
        if(lines.Count == 0)
            return FrozenSet<DeclaredItemDraft>.Empty;

        var result = new HashSet<DeclaredItemDraft>();

        void AddDeclaration(string? s, StringBuilder stringBuilder)
        {
            if (string.IsNullOrEmpty(s))
                throw new Exception("Declaration name not defined");

            string p = stringBuilder.ToString();
            if (string.IsNullOrEmpty(p))
                throw new Exception($"Declaration '{s}' not initialized");

            var (name, typeStr) = s.SplitToTwoParts(Consts.ControlChars.SetType).Trim();

            ReturnType? type = typeStr != null ? ReturnType.Parse(typeStr) : null;

            result.AddOrThrowIfContains(new DeclaredItemDraft(name, p, type, declarationSource));
        }

        string? variableName = null;
        var declarePattern = new StringBuilder();

        bool isFirstLine = true;
        foreach (var line in lines)
        {
            if (IsDefineDeclaration(line, [RuleVariable.Prefix, Function.Prefix]))
            {
                if (!isFirstLine)
                {
                    AddDeclaration(variableName, declarePattern);
                    declarePattern.Clear();
                }

                var (name, pattern) = line.SplitToTwoParts(Consts.ControlChars.AssignmentOperator).Trim();
                variableName = name;
                declarePattern.Append(pattern);
            }
            else
            {
                declarePattern.Append((declarePattern.Length > 0 ? Constants.NewLine : "") + line);
            }

            isFirstLine = false;
        }

        AddDeclaration(variableName, declarePattern);

        return result;
    }


    // public async Task<DeclaredItems> Parse(IReadOnlyCollection<string> lines, IReadonlyParsingContext parsingContext)
    // {
    //     if (lines.Count == 0)
    //         return new DeclaredItems();
    //
    //     var all = DeclaredItems.WithoutLocalVariables(parsingContext.DeclaredItems);
    //     var newContext = new ParsingContext(parsingContext, declaredItems: all);
    //
    //     var onlyNew = new DeclaredItems();
    //
    //     foreach (var (nameWithType, pattern) in GetNameToPatternMap(lines, RuleVariable.Prefix, Function.Prefix))
    //     {
    //         ObjectTextParts parts = await textPartsParser.Parse(pattern, newContext);
    //
    //         ReturnType? type = null;
    //         var (name, typeStr) = nameWithType.SplitToTwoParts(Consts.ControlChars.SetType).Trim();
    //
    //         if (typeStr != null)
    //             type = ReturnType.Parse(typeStr);
    //         else if (parts.IsString)
    //             type = ReturnType.String;
    //
    //         if (name.StartsWith(RuleVariable.Prefix))
    //         {
    //             var variable = new DeclaredRuleVariable(name, parts, type);
    //             all.AddOrThrowIfContains(variable);
    //             onlyNew.AddOrThrowIfContains(variable);
    //         }
    //         else if (name.StartsWith(Function.Prefix))
    //         {
    //             var function = new Function(name, parts, type);
    //             all.AddOrThrowIfContains(function);
    //             onlyNew.AddOrThrowIfContains(function);
    //         }
    //         else
    //         {
    //             throw new Exception($"Unknown declaration type: '{name}'");
    //         }
    //     }
    //
    //     return onlyNew;
    // }

    // private static Dictionary<string, string> GetNameToPatternMap(IReadOnlyCollection<string> lines, params string[] namePrefixes)
    // {
    //     var nameToValueMap = new Dictionary<string, string>();
    //
    //     void AddVariable(string? s, StringBuilder stringBuilder)
    //     {
    //         if (string.IsNullOrEmpty(s))
    //             throw new Exception("Variable name not defined");
    //
    //         string p = stringBuilder.ToString();
    //         if (string.IsNullOrEmpty(p))
    //             throw new Exception($"Variable '{s}' not initialized");
    //
    //         nameToValueMap.Add(s, stringBuilder.ToString());
    //     }
    //
    //     string? variableName = null;
    //     var variablePattern = new StringBuilder();
    //
    //     bool isFirstLine = true;
    //     foreach (var line in lines)
    //     {
    //         if (IsDefineVariable(line, namePrefixes))
    //         {
    //             if (!isFirstLine)
    //             {
    //                 AddVariable(variableName, variablePattern);
    //                 variablePattern.Clear();
    //             }
    //
    //             var (name, pattern) = line.SplitToTwoParts(Consts.ControlChars.AssignmentOperator).Trim();
    //             variableName = name;
    //             variablePattern.Append(pattern);
    //         }
    //         else
    //         {
    //             variablePattern.Append((variablePattern.Length > 0 ? Constants.NewLine : "") + line);
    //         }
    //
    //         isFirstLine = false;
    //     }
    //
    //     AddVariable(variableName, variablePattern);
    //
    //     return nameToValueMap;
    // }

    private static bool IsDefineDeclaration(string line, string[] namePrefixes)
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