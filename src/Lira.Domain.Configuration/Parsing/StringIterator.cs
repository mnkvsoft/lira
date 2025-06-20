using System.Diagnostics.CodeAnalysis;

class StringIterator
{
    private string _source;
    private int _index;

    public StringIterator(string source)
    {
        _source = source;
    }

    public char Current { get; }

    public bool MoveTo(char c)
    {

        throw new NotImplementedException();
    }


    public bool MoveTo(char c, Func<char, bool> available, out char unavailable)
    {

        throw new NotImplementedException();
    }

    public bool MoveTo(char c, Func<char, bool> available)
    {

        throw new NotImplementedException();
    }

    public bool MoveTo(Func<char, bool> predicate)
    {

        throw new NotImplementedException();
    }


    public bool MoveNext()
    {
        throw new NotImplementedException();
    }

    public bool NextIs(char s)
    {
        throw new NotImplementedException();
    }

    public string Pop()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Перемотать вперед до индекса, где следующий символ удовлетворяет условию
    /// </summary>
    /// <param name="predicate"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void ForwardWhereNext(Func<char, bool> predicate)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Далее идет одна из строк указанных в списке
    /// </summary>
    public bool NextOneOf(ISet<string> values, [MaybeNullWhen(false)] out string foundValue)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Смещает индек к концу этого значения
    /// </summary>
    public void ForwardToEnd(string value)
    {
        throw new NotImplementedException();
    }

    public string Peek()
    {
        throw new NotImplementedException();
    }

    public bool NextIs(string end)
    {
        throw new NotImplementedException();
    }
}