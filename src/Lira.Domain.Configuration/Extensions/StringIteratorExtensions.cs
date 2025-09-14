using Lira.Common;

namespace Lira.Domain.Configuration.Extensions;

static class StringIteratorExtensions
{
    public static string MoveToNextParameterOrEnd(this StringIterator iterator)
    {
        string? result;
        if (iterator.MoveTo(','))
        {
            result = iterator.PopExcludeCurrent();
            iterator.PopIncludeCurrent();
        }
        else
        {
            iterator.MoveToEnd();
            result = iterator.PopIncludeCurrent();
        }
        return result?.Trim() ?? throw new Exception("value is null");
    }

    public static bool MoveToCloseStringParameterValue(this StringIterator iterator)
    {
        while (iterator.MoveNext())
        {
            // дальше идет экранироение, просто пропускаем
            var cur = iterator.Current;
            if (cur == '\\')
            {
                if (iterator.HasNext())
                {
                    iterator.MoveNext();
                }
                else
                {
                    return false;
                }
            }

            if (cur == '"')
            {
                return true;
            }
        }

        return false;
    }
}