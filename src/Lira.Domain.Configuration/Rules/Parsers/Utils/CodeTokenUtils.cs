using Lira.Common.Extensions;
using Lira.Domain.Configuration.Extensions;
using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.Domain.TextPart;
using Lira.Domain.TextPart.Impl.Custom;
using Lira.Domain.TextPart.Impl.Custom.VariableModel.Impl;

namespace Lira.Domain.Configuration.Rules.Parsers.Utils;

internal static class CodeTokenUtils
{
    public static void HandleTokenWithAccessToItem(string invoke, ParsingContext context, IReadOnlyCollection<CodeToken> codeTokens)
    {
        foreach (var codeToken in codeTokens)
        {
            if (codeToken is CodeToken.ReadItem readItem)
            {
                if(!context.DeclaredItems.Exists(readItem.ItemName))
                    throw new Exception($"Unknown declaration '{readItem.ItemName}'");
            }
            else if (codeToken is CodeToken.WriteItem writeItem)
            {
                if(writeItem.ItemName.StartsWith(Consts.ControlChars.FunctionPrefix))
                    throw new Exception($"Function '{writeItem.ItemName}' cannot be assigned a value. Code: '{invoke}'");

                var variableName = writeItem.ItemName.TrimStart(Consts.ControlChars.VariablePrefix);
                context.DeclaredItems.Variables.Add(new RuntimeVariable(new CustomItemName(variableName), type: null));
            }
        }
    }
}