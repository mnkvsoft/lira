using System.Text;
using Lira.Common.Exceptions;
using Lira.Common.Extensions;
using Lira.Domain.Configuration.Extensions;
using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.Domain.TextPart;
using Lira.Domain.TextPart.Impl.Custom;
using Lira.Domain.TextPart.Impl.Custom.VariableModel.Impl;

namespace Lira.Domain.Configuration.Rules.Parsers.CodeParsing;

static class CodeParser
{
    public static (CodeBlock, IReadOnlyCollection<RuntimeVariable>) Parse(string code,
        IReadonlyDeclaredItems declaredItems)
    {
        try
        {
            return ParseInternal(code, declaredItems);
        }
        catch (Exception e)
        {
            var nl = Environment.NewLine;
            throw new Exception(e.Message + nl + "Error parsing code: " + nl +
                                code.WrapBeginEnd() +
                                "Declared items: " + nl +
                                declaredItems,
                e);
        }
    }

    private static (CodeBlock, IReadOnlyCollection<RuntimeVariable>) ParseInternal(string code, IReadonlyDeclaredItems declaredItems)
    {
        var codeTokens = Parse(code);

        // case: $$some.var.ToString()
        var result = new List<CodeToken>(codeTokens.Count * 2);

        var onlyNewVariables = new List<RuntimeVariable>();

        var withNewVariables = new List<DeclaredItem>();
        withNewVariables.AddRange(declaredItems);

        foreach (var codeToken in codeTokens)
        {
            if (codeToken is CodeToken.ReadItem readItem)
            {
                var names = withNewVariables
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

                    var variable = new RuntimeVariable(new CustomItemName(variableName), valueType: null);

                    onlyNewVariables.Add(variable);
                    withNewVariables.Add(variable);

                    result.Add(writeItem);
                }
                else
                {
                    throw new Exception($"Function '{writeItem.ItemName}' cannot be assigned a value. Code: '{code}'");
                }
            }
            else if (codeToken is CodeToken.OtherCode otherCode)
            {
                result.Add(otherCode);
            }
            else
            {
                throw new UnsupportedInstanceType(codeToken);
            }
        }

        result = ConcatNeighboringOtherCodeTokens(result);

        return (new CodeBlock(result), onlyNewVariables);
    }

    private static List<CodeToken> ConcatNeighboringOtherCodeTokens(List<CodeToken> result)
    {
        var newResult = new List<CodeToken>();

        using var enumerator = result.GetEnumerator();

        var newToken = new StringBuilder();
        while (enumerator.MoveNext())
        {
            if (enumerator.Current is not CodeToken.OtherCode otherCode)
            {
                if (newToken.Length > 0)
                {
                    newResult.Add(new CodeToken.OtherCode(newToken.ToString()));
                    newToken.Clear();
                }

                newResult.Add(enumerator.Current);
            }
            else
            {
                newToken.Append(otherCode.Code);
            }
        }

        if (newToken.Length > 0)
        {
            newResult.Add(new CodeToken.OtherCode(newToken.ToString()));
        }

        return newResult;
    }

    private static IReadOnlyCollection<CodeToken> Parse(string code)
    {
        var tokens = new List<CodeToken>();
        bool enterVariableName = false;

        using var iterator = code.GetEnumerator();
        var sbAccessToVariable = new StringBuilder();
        var sbOtherCode = new StringBuilder();

        while (iterator.MoveNext())
        {
            sbOtherCode.Append(iterator.Current);
            if (enterVariableName)
            {
                if (CustomItemName.IsAllowedCharInName(iterator.Current))
                {
                    sbAccessToVariable.Append(iterator.Current);
                }
                else
                {
                    // case: $$variable=
                    if (iterator.Current == '=')
                    {
                        HandleEqualsOperator();
                    }
                    else
                    {
                        // case: $$variable[whitespace]
                        if (iterator.Current == ' ')
                        {
                            // ignore whitespaces chars
                            while (iterator.MoveNext())
                            {
                                sbOtherCode.Append(iterator.Current);
                                if(iterator.Current != ' ')
                                    break;
                            }

                            // case: $$variable     =
                            if (iterator.Current == '=')
                            {
                                HandleEqualsOperator();
                            }
                            // case: $$variable     ;
                            else
                            {
                                AddReadVariableToken();
                            }
                        }
                        // case: $$variable;
                        else
                        {
                            AddReadVariableToken();
                        }
                    }
                }
            }
            else
            {
                if (iterator.Current == '$')
                {
                    sbAccessToVariable.Append(iterator.Current);

                    iterator.MoveNext();
                    sbOtherCode.Append(iterator.Current);

                    if (iterator.Current == '$')
                    {
                        sbAccessToVariable.Append(iterator.Current);

                        iterator.MoveNext();
                        sbOtherCode.Append(iterator.Current);
                    }

                    if (CustomItemName.IsAllowedFirstCharInName(iterator.Current))
                    {
                        enterVariableName = true;
                        sbAccessToVariable.Append(iterator.Current);
                        if (sbOtherCode.Length > 0)
                        {
                            int length = sbOtherCode.Length - sbAccessToVariable.Length;
                            if (length > 0)
                            {
                                tokens.Add(new CodeToken.OtherCode(sbOtherCode.ToString()[..length]));
                            }

                            sbOtherCode.Clear();
                        }
                    }
                    else
                    {
                        sbAccessToVariable.Clear();
                    }
                }
            }
        }

        if (enterVariableName)
            tokens.Add(new CodeToken.ReadItem(sbAccessToVariable.ToString()));
        else
            tokens.Add(new CodeToken.OtherCode(sbOtherCode.ToString()));

        return tokens;

        void HandleEqualsOperator()
        {
            iterator.MoveNext();
            sbOtherCode.Append(iterator.Current);

            // case: if($$variable== "value")
            if (iterator.Current == '=')
            {
                tokens.Add(new CodeToken.ReadItem(sbAccessToVariable.ToString()));
                sbAccessToVariable.Clear();
                sbOtherCode.Clear().Append("==");
            }
            // case: $$variable= "value"
            else
            {
                tokens.Add(new CodeToken.WriteItem(sbAccessToVariable.ToString()));
                sbAccessToVariable.Clear();
                sbOtherCode.Remove(0, sbOtherCode.Length - 2);
            }

            enterVariableName = false;
        }

        void AddReadVariableToken()
        {
            tokens.Add(new CodeToken.ReadItem(sbAccessToVariable.ToString()));
            sbAccessToVariable.Clear();
            sbOtherCode.Clear().Append(iterator.Current);
            enterVariableName = false;
        }
    }
}
