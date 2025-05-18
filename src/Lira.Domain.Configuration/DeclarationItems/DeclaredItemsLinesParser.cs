using System.Collections.Frozen;
using System.Text;
using Lira.Common;
using Lira.Common.Extensions;
using Lira.Domain.TextPart;
using Lira.Domain.TextPart.Impl.Custom.FunctionModel;
using Lira.Domain.TextPart.Impl.Custom.VariableModel.RuleVariables;

namespace Lira.Domain.Configuration.DeclarationItems;

class DeclaredItemsLinesParser
{
    public IReadOnlySet<DeclaredItemDraft> Parse(IReadOnlyCollection<string> lines)
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

            result.AddOrThrowIfContains(new DeclaredItemDraft(name, p, type));
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