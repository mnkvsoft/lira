using System.Text;

namespace SimpleMockServer.Domain.Functions.Native;

internal static class CallChainParser
{
    public static IReadOnlyList<MethodCall> Parse(string chain)
    {
        if (string.IsNullOrWhiteSpace(chain))
            throw new CallChainParsingException("Empty chain");

        List<MethodCall> result = new List<MethodCall>();
        var enumerator = chain.GetEnumerator();


        StringBuilder call = new StringBuilder();
        bool isArgs = false;
        bool waitNextInvoke = false;
        bool waitArgumentsBlock = false;

        while (enumerator.MoveNext())
        {
            if (waitNextInvoke)
            {
                if (enumerator.Current == ' ')
                {

                }
                else if (enumerator.Current == '.')
                {
                    waitArgumentsBlock = false;
                    waitNextInvoke = false;
                    isArgs = false;
                }
                else
                    throw new CallChainParsingException($"Expected '.' but '{enumerator.Current}'");
            }
            else if (!isArgs)
            {
                if (call.Length == 0)
                {
                    if (enumerator.Current == '(')
                    {
                        throw new CallChainParsingException("Function name is empty");
                    }
                    else if (enumerator.Current == ' ')
                    {

                    }
                    else
                    {
                        call.Append(enumerator.Current);
                    }
                }
                else
                {
                    if (waitArgumentsBlock && enumerator.Current == ' ')
                    {
                    }
                    else if (enumerator.Current == ' ')
                    {
                        waitArgumentsBlock = true;
                    }
                    else if (enumerator.Current == '(')
                    {
                        isArgs = true;
                    }
                    else
                    {
                        if (waitArgumentsBlock)
                            throw new CallChainParsingException($"Expected '(' but '{enumerator.Current}'");
                        else
                            call.Append(enumerator.Current);
                    }
                }

            }
            else if (isArgs)
            {
                if (enumerator.Current != ' ')
                {
                    List<string> args = GetArgs(enumerator);
                    // текущий символ: )

                    result.Add(new MethodCall(call.ToString(), args));
                    call.Clear();
                    waitNextInvoke = true;
                }
            }
            else
                throw new CallChainParsingException("unknown section");
        }

        if (call.Length != 0)
        {
            if (!isArgs)
                throw new CallChainParsingException($"Missing brackets in '{call}' function");

            if (isArgs)
                throw new CallChainParsingException($"Missing close bracket in {call} function");

        }
        return result;

    }

    private static List<string> GetArgs(CharEnumerator enumerator)
    {
        List<string> args = new List<string>();

        bool isFirst = true;
        bool waitNextArg = false;

        while (isFirst || enumerator.MoveNext())
        {
            isFirst = false;

            if (enumerator.Current != ' ')
            {
                waitNextArg = false;

                if (enumerator.Current == ')')
                    break;

                string arg = GetArg(enumerator);
                args.Add(arg);

                if (enumerator.Current == ')')
                    break;

                if (enumerator.Current == ',')
                    waitNextArg = true;
            }
        }

        if (waitNextArg)
            throw new CallChainParsingException("Missing ')'");

        return args;
    }

    private static string GetArg(CharEnumerator enumerator)
    {
        // вход на первом символе аргумента

        bool isString = false;
        StringBuilder arg = new StringBuilder();
        if (enumerator.Current == '\'')
            isString = true;
        else
            arg.Append(enumerator.Current);

        bool isEnd = false;


        while (enumerator.MoveNext())
        {
            if (isEnd)
            {
                if (enumerator.Current == ' ')
                {

                }
                else if (enumerator.Current == ',' || enumerator.Current == ')')
                {
                    break;
                }
                else
                {
                    throw new CallChainParsingException($"Unexpected '{enumerator.Current}'");
                }
            }
            else if (isString)
            {
                if (enumerator.Current == '\'')
                {
                    isEnd = true;
                }
                else
                {
                    arg.Append(enumerator.Current);
                }
            }
            else
            {
                if (enumerator.Current == ',' || enumerator.Current == ')')
                {
                    isEnd = true;
                    break;
                }
                if (enumerator.Current == ' ')
                {
                    isEnd = true;
                }
                else
                {
                    arg.Append(enumerator.Current);
                }
            }

        }

        if (!isEnd)
            throw new CallChainParsingException($"Not ended argument: '{arg}'");
        return arg.ToString();

    }
}