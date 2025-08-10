namespace Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Parsing;

static class PatternPartsIteratorExtensions
{
    public static void MoveToNewlineAndPop(this PatternPartsIterator iterator)
    {
        if (iterator.MoveTo(currentPredicate: c => c == '\n', available: char.IsWhiteSpace))
            iterator.PopIncludeCurrent();
    }
}