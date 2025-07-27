namespace Lira.Common;

public static class StringIteratorExtensions
{
    public static void MoveToNewlineAndPop(this StringIterator iterator)
    {
        if (iterator.MoveTo(currentPredicate: c => c == '\n', available: char.IsWhiteSpace))
            iterator.PopIncludeCurrent();
    }
}