using System.Text;
using Lira.Common;
using Lira.Common.Exceptions;
using Lira.Common.Extensions;
using Lira.Domain.TextPart;
using Lira.Domain.TextPart.Impl.Custom;
using Lira.Domain.TextPart.Impl.Custom.FunctionModel;
using Lira.Domain.TextPart.Impl.Custom.VariableModel.LocalVariables;
using Lira.Domain.TextPart.Impl.Custom.VariableModel.RuleVariables;
using Lira.Domain.TextPart.Impl.Custom.VariableModel.RuleVariables.Impl;

namespace Lira.Domain.Configuration.Rules.Parsers.CodeParsing;

class CodeParser
{
    private readonly IReadOnlyCollection<string> _keyWords;

    public CodeParser(IEnumerable<IKeyWordInDynamicBlock> keyWordProviders)
    {
        _keyWords = keyWordProviders.Select(x => x.Word).ToArray();
    }

    public (CodeBlock, IReadOnlyCollection<RuntimeRuleVariable>, IReadOnlyCollection<LocalVariable>) Parse(
        string code,
        IReadOnlySet<DeclaredItem> declaredItems)
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

    private (CodeBlock, IReadOnlyCollection<RuntimeRuleVariable>, IReadOnlyCollection<LocalVariable>) ParseInternal(string code,
        IReadOnlySet<DeclaredItem> declaredItems)
    {
        var codeTokens = Parse(code);

        // case: $$some.var.ToString()
        var result = new List<CodeToken>(codeTokens.Count * 2);

        var onlyNewVariables = new List<RuntimeRuleVariable>();
        var newLocalVariables = new List<LocalVariable>();

        var withNewVariables = new List<DeclaredItem>();
        withNewVariables.AddRange(declaredItems);

        foreach (var codeToken in codeTokens)
        {
            if (codeToken is CodeToken.ReadItem readItem)
            {
                var names = withNewVariables
                    .Where(item => readItem.ItemName.StartsWith(item.Name))
                    .Select(item => item.Name);

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
                if (writeItem.ItemName.StartsWith(RuleVariable.Prefix))
                {
                    var variable = new RuntimeRuleVariable(writeItem.ItemName, valueType: null);

                    onlyNewVariables.Add(variable);
                    withNewVariables.Add(variable);

                    result.Add(writeItem);
                }
                else if (writeItem.ItemName.StartsWith(LocalVariable.Prefix))
                {
                    var variable = new LocalVariable(writeItem.ItemName, valueType: null);

                    newLocalVariables.Add(variable);
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

        return (new CodeBlock(result), onlyNewVariables, newLocalVariables);
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

    private IReadOnlyCollection<CodeToken> Parse(string code)
    {
        var tokens = new List<CodeToken>();
        NamingStrategy? itemNamingStrategy = null;

        var iterator = new StringIterator(code);
        var sbAccessToVariable = new StringBuilder();
        var sbOtherCode = new StringBuilder();

        while (iterator.MoveNext())
        {
            sbOtherCode.Append(iterator.Current);
            if (itemNamingStrategy != null)
            {
                if (itemNamingStrategy.IsAllowedChar(iterator.Current))
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
                if (iterator.Current == '@' && iterator.NextIsOneOf(_keyWords.Select(x => x + " "), out var word))
                {
                    sbOtherCode.Append(word);
                    iterator.MoveToEnd(word);
                }
                else if (iterator.Current == '$')
                {
                    var namingStrategy = LocalVariable.NamingStrategy;
                    sbAccessToVariable.Append(iterator.Current);

                    iterator.MoveNext();
                    sbOtherCode.Append(iterator.Current);

                    if (iterator.Current == '$')
                    {
                        namingStrategy = RuleVariable.NamingStrategy;
                        sbAccessToVariable.Append(iterator.Current);

                        iterator.MoveNext();
                        sbOtherCode.Append(iterator.Current);
                    }

                    if (namingStrategy.IsAllowedFirstChar(iterator.Current))
                    {
                        itemNamingStrategy = namingStrategy;
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
                else if (iterator.Current == '@')
                {
                    var namingStrategy = Function.NamingStrategy;
                    sbAccessToVariable.Append(iterator.Current);

                    iterator.MoveNext();
                    sbOtherCode.Append(iterator.Current);

                    if (namingStrategy.IsAllowedFirstChar(iterator.Current))
                    {
                        itemNamingStrategy = namingStrategy;
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

        if (itemNamingStrategy != null)
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

            itemNamingStrategy = null;
        }

        void AddReadVariableToken()
        {
            tokens.Add(new CodeToken.ReadItem(sbAccessToVariable.ToString()));
            sbAccessToVariable.Clear();
            sbOtherCode.Clear().Append(iterator.Current);
            itemNamingStrategy = null;
        }
    }
}
