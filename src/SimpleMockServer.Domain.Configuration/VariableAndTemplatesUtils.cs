using System.Text;
using SimpleMockServer.Common.Extensions;

namespace SimpleMockServer.Domain.Configuration;

public static class VariableAndTemplatesUtils
{
    public static Dictionary<string, string> GetNameToPatternMap(IReadOnlyCollection<string> lines, char namePrefix)
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
            if (IsDefineVariable(line, namePrefix))
            {
                if (!isFirstLine)
                {
                    AddVariable(variableName, variablePattern);
                    variablePattern.Clear();
                }

                var (name, pattern) = line.SplitToTwoParts(Consts.ControlChars.AssignmentOperator).Trim();
                variableName = name.TrimStart(namePrefix);
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

    private static bool IsDefineVariable(string line, char namePrefix)
    {
        if (line.StartsWith(namePrefix))
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