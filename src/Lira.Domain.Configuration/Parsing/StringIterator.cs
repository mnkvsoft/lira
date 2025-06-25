// ReSharper disable ReplaceWithPrimaryConstructorParameter
namespace Lira.Domain.Configuration.Parsing;

class StringIterator(string source)
{
    private readonly string _source = source;

    private readonly int _lastIndex = source.Length - 1;
    private int _currentIndex = -1;
    private int _lastPopIndex = -1;

    public char Current => _source[_currentIndex];
    private char Next => _source[NextIndex];
    private int NextIndex
    {
        get
        {
            if (!HasNext())
                throw new InvalidOperationException("End of line reached");

            return _currentIndex + 1;
        }
    }

    public bool HasNext() => _source.Length > 0 && _currentIndex <= _lastIndex;

    public bool MoveTo(char c) => MoveTo(ch => ch == c);

    public bool MoveTo(char c, Func<char, bool> available) => MoveTo(currentPredicate: ch => ch == c, available: available);

    public bool MoveTo(Func<char, bool>? currentPredicate = null, Func<char, bool>? nextPredicate = null, Func<char, bool>? available = null)
    {
        if(currentPredicate == null && nextPredicate == null)
            throw new ArgumentException($"'{nameof(currentPredicate)}' and '{nameof(nextPredicate)}' should not be null.");

        if (_source.Length == 0)
            return false;

        var index = _currentIndex + 1;
        while (index <= _lastIndex)
        {
            int nextIndex = index + 1;
            var hasNext = nextIndex <= _lastIndex;

            if ((currentPredicate?.Invoke(_source[index]) ?? true) && (!hasNext || (nextPredicate?.Invoke(_source[nextIndex]) ?? true)))
            {
                _currentIndex = index;
                return true;
            }

            if(available?.Invoke(_source[index]) == false)
            {
                return false;
            }
            index++;
        }

        return false;
    }

    public bool MoveNext()
    {
        if (_source.Length == 0)
            return false;

        if(_currentIndex < _lastIndex)
        {
            _currentIndex++;
            return true;
        }

        return false;
    }

    public bool NextIs(char c) => NextIs(ch => ch == c);

    private bool NextIs(Func<char, bool> predicate)
    {
        if (_source.Length == 0)
            return false;

        if (!HasNext())
            return false;

        return predicate(Next);
    }

    /// <summary>
    /// Перемотать вперед до индекса, где следующий символ удовлетворяет условию
    /// </summary>
    public bool MoveToWhereNext(Func<char, bool> nextPredicate) => MoveTo(nextPredicate: nextPredicate);

    /// <summary>
    /// Смещает индек к концу этого значения
    /// </summary>
    public bool MoveToEnd(string value)
    {
        if (NextIs(value))
        {
            _currentIndex += value.Length;
            return true;
        }

        return false;
    }

    public bool NextIs(string value)
    {
        if (!HasNext())
            return false;

        int endIndex = _currentIndex + value.Length;
        if (endIndex > _lastIndex)
            return false;

        return _source.Substring(NextIndex, value.Length) == value;
    }

    /// <summary>
    /// Смещает индек к концу этого значения
    /// </summary>
    public void MoveToEnd() =>_currentIndex = _lastIndex;

    public string Peek() => _source.Substring(_lastPopIndex, _currentIndex - _lastPopIndex);

    public string? PopExcludeCurrent()
    {
        var startIndex = _lastPopIndex == -1 ? 0 : _lastPopIndex;

        if(startIndex == _currentIndex)
            return null;

        var result = _source.Substring(startIndex, _currentIndex - startIndex);
        _lastPopIndex = _currentIndex;
        return result;
    }

    public string? PopIncludeCurrent()
    {
        var startIndex = _lastPopIndex == -1 ? 0 : _lastPopIndex;

        int currentIndex = _currentIndex + 1;
        if(startIndex == currentIndex)
            return null;

        var result = _source.Substring(startIndex, currentIndex - startIndex);
        _lastPopIndex = currentIndex;
        return result;
    }
}