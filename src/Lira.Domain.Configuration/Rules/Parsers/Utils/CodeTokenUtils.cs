using Lira.Common.Exceptions;
using Lira.Common.Extensions;
using Lira.Domain.Configuration.Extensions;
using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.Domain.TextPart;
using Lira.Domain.TextPart.Impl.Custom;
using Lira.Domain.TextPart.Impl.Custom.VariableModel.Impl;

namespace Lira.Domain.Configuration.Rules.Parsers.Utils;

internal static class CodeTokenUtils
{
    public static CodeBlock HandleTokenWithAccessToItem(string invoke, ParsingContext context, IReadOnlyCollection<CodeToken> codeTokens)
    {
        var result = new List<CodeToken>(codeTokens.Count * 2);

        foreach (var codeToken in codeTokens)
        {
            if (codeToken is CodeToken.ReadItem readItem)
            {
                var names = context.DeclaredItems
                    .Where(item => readItem.ItemName.StartsWith(item.GetFullName()))
                    .Select(item => item.GetFullName());

                string? readItemName = null;
                foreach (var name in names)
                {
                    readItemName ??= name;

                    if (readItemName.Length < name.Length)
                        readItemName = name;
                }

                if(readItemName == null)
                    throw new Exception($"Unknown declaration '{readItem.ItemName}'");

                if (readItemName == readItem.ItemName)
                {
                    result.Add(codeToken);
                }
                else
                {
                    result.Add(new CodeToken.ReadItem(readItemName));
                    result.Add(new CodeToken.OtherCode(readItem.ItemName[readItemName.Length..]));
                }
            }
            else if (codeToken is CodeToken.WriteItem writeItem)
            {
                if (writeItem.ItemName.StartsWith(Consts.ControlChars.VariablePrefix))
                {
                    var variableName = writeItem.ItemName.TrimStart(Consts.ControlChars.VariablePrefix);
                    context.DeclaredItems.Variables.Add(new RuntimeVariable(new CustomItemName(variableName), type: null));
                    result.Add(writeItem);
                }
                else
                {
                    throw new Exception($"Function '{writeItem.ItemName}' cannot be assigned a value. Code: '{invoke}'");
                }
            }
            else if (codeToken is CodeToken.OtherCode code)
            {
                result.Add(code);
            }
            else
            {
                throw new UnsupportedInstanceType(codeToken);
            }
        }

        return new CodeBlock(result);
    }
}