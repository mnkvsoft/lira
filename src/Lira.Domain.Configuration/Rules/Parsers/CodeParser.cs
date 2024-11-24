using System.Text;
using Lira.Domain.TextPart.Impl.Custom;

namespace Lira.Domain.Configuration.Rules.Parsers;

public abstract record CodeToken
{
    public record WriteVariable(string ItemName) : CodeToken
    {
        public override string ToString() => $"[:w {ItemName}]";
    }
    public record ReadVariable(string ItemName) : CodeToken
    {
        public override string ToString() => $"[:r {ItemName}]";
    }
    public record OtherCode(string Code) : CodeToken
    {
        public override string ToString() => $"[:c {Code}]";
    }
}

public static class CodeParser
{
    public static IReadOnlyList<CodeToken> Parse(string code)
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
                    }

                    iterator.MoveNext();
                    sbOtherCode.Append(iterator.Current);

                    if (CustomItemName.IsAllowedCharInName(iterator.Current))
                    {
                        enterVariableName = true;
                        sbAccessToVariable.Append(iterator.Current);
                        if (sbOtherCode.Length > 0)
                        {
                            tokens.Add(new CodeToken.OtherCode(sbOtherCode.ToString()[..(sbOtherCode.Length - sbAccessToVariable.Length)]));
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
            tokens.Add(new CodeToken.ReadVariable(sbAccessToVariable.ToString()));
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
                tokens.Add(new CodeToken.ReadVariable(sbAccessToVariable.ToString()));
                sbAccessToVariable.Clear();
                sbOtherCode.Clear().Append("==");
            }
            // case: $$variable= "value"
            else
            {
                sbAccessToVariable.Append('=');
                tokens.Add(new CodeToken.WriteVariable(sbAccessToVariable.ToString()));
                sbAccessToVariable.Clear();
                sbOtherCode.Clear().Append(iterator.Current);
            }

            enterVariableName = false;
        }

        void AddReadVariableToken()
        {
            tokens.Add(new CodeToken.ReadVariable(sbAccessToVariable.ToString()));
            sbAccessToVariable.Clear();
            sbOtherCode.Clear().Append(iterator.Current);
            enterVariableName = false;
        }
    }
}
