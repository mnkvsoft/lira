using System.Diagnostics.CodeAnalysis;
using Lira.Common;
using Lira.Common.Exceptions;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Parsing;

class PatternPartsIterator
{
    private readonly IReadOnlyList<PatternPart> _source;

    private int _index = -1;
    private readonly int _lastIndex;
    private StringIterator? _currentIterator;

    private int _lastPopIndex = -1;
    private int _lastPopIndexInPart = -1;

    public PatternPartsIterator(PatternParts source)
    {
        if (source.Count == 0)
            throw new ArgumentException("The source cannot be empty", nameof(source));

        _source = source;
        _lastIndex = source.Count - 1;

        if (_source[0] is PatternPart.Static st)
            _currentIterator = new StringIterator(st.Value);
    }

    private PatternPartsIterator(PatternPartsIterator iterator)
    {
        _source = iterator._source;
        _index = iterator._index;
        _lastIndex = iterator._lastIndex;
        _currentIterator = iterator._currentIterator;
        _lastPopIndex = iterator._lastPopIndex;
        _lastPopIndexInPart = iterator._lastPopIndexInPart;
    }

    public char Current => GetStaticPartIterator().Current;

    public bool MoveTo(Func<char, bool>? currentPredicate = null, Func<char, bool>? nextPredicate = null,
        Func<char, bool>? available = null)
    {
        if (currentPredicate == null && nextPredicate == null)
            throw new ArgumentException(
                $"'{nameof(currentPredicate)}' and '{nameof(nextPredicate)}' should not be null.");

        if (_source.Count == 0)
            return false;

        var index = _index;
        var iterator = _currentIterator?.Clone();

        while (index <= _lastIndex)
        {
            if (index == -1)
                index++;

            if (iterator != null)
            {
                if (iterator.MoveTo(currentPredicate, nextPredicate, available))
                {
                    _index = index;
                    _currentIterator = iterator;
                    return true;
                }
                else
                {
                    iterator = null;
                    index++;
                }
            }
            else
            {
                var part = _source[index];
                if (part is PatternPart.Dynamic)
                {
                    index++;
                }
                else if (part is PatternPart.Static st)
                {
                    iterator = new StringIterator(st.Value);
                }
                else
                {
                    throw new UnsupportedInstanceType(part);
                }
            }
        }

        return false;
    }

    public void MoveBackTo(char c) => MoveBackTo(ch => ch == c);

    public bool MoveBackTo(Func<char, bool> currentPredicate)
    {
        if (_source.Count == 0)
            return false;

        while (_index >= 0)
        {
            if (_currentIterator != null)
            {
                if (_currentIterator.MoveBackTo(currentPredicate))
                {
                    return true;
                }
                else
                {
                    _currentIterator = null;
                    _index--;
                }
            }
            else
            {
                var part = _source[_index];
                if (part is PatternPart.Dynamic)
                {
                    _index--;
                }
                else if (part is PatternPart.Static st)
                {
                    _currentIterator = new StringIterator(st.Value);
                }
                else
                {
                    throw new UnsupportedInstanceType(part);
                }
            }
        }

        return false;
    }

    public bool MoveTo(char c)
    {
        return MoveTo(currentPredicate: ch => ch == c);
    }

    public bool MoveToWhereNext(Func<char, bool> nextPredicate) => MoveTo(nextPredicate: nextPredicate);


    public PatternParts PopExcludeCurrent() => Pop(includeCurrent: false);
    public PatternParts PopIncludeCurrent() => Pop(includeCurrent: true);

    private PatternParts Pop(bool includeCurrent) => PopOrPeek(includeCurrent, pop: true);

    public PatternParts Peek() => PopOrPeek(includeCurrent: false, pop: false);

    private PatternParts PopOrPeek(bool includeCurrent, bool pop)
    {
        var result = new List<PatternPart>();

        StringIterator? iterator;


        if (_index == _lastPopIndex)
        {
            iterator = GetStaticPartIterator();
            AddIfNotNull(iterator.Pop(includeCurrent, out var index));

            if (pop)
                _lastPopIndexInPart = index;
        }
        else
        {
            if (_lastPopIndex != -1 && _source[_lastPopIndex] is PatternPart.Static st)
            {
                result.Add(new PatternPart.Static(st.Value[_lastPopIndexInPart..]));

                if (pop)
                    _lastPopIndexInPart = -1;
            }

            for (int i = _lastPopIndex + 1; i < _index; i++)
            {
                var part = _source[i];
                result.Add(part);

                if (pop)
                    _lastPopIndex = i;
            }

            if (CurrentPartIsStatic(out iterator))
            {
                AddIfNotNull(iterator.Pop(includeCurrent, out var index));
                if (pop)
                {
                    _lastPopIndexInPart = index;
                    _lastPopIndex = _index;
                }
            }
        }

        void AddIfNotNull(string? value)
        {
            if (value != null)
                result.Add(new PatternPart.Static(value));
        }

        return new PatternParts(result);
    }

    public bool NextIs(char c)
    {
        if (CurrentPartIsStatic(out var iterator))
            return iterator.NextIs(c);
        return false;
    }

    public bool NextIs(string str)
    {
        if (CurrentPartIsStatic(out var iterator))
            return iterator.NextIs(str);
        return false;
    }

    public void MoveToEnd()
    {
        if (_index != _lastIndex)
        {
            _index = _lastIndex;
            _currentIterator = null;

            if (_source[_index] is PatternPart.Static st)
            {
                _currentIterator = new StringIterator(st.Value);
                _currentIterator.MoveToEnd();
            }
        }
        else
        {
            if (CurrentPartIsStatic(out var iterator))
            {
                iterator.MoveToEnd();
            }
        }
    }

    public bool MoveNext() => MoveTo(currentPredicate: _ => true);

    public bool MoveToEnd(string value)
    {
        if (CurrentPartIsStatic(out var iterator))
            return iterator.MoveToEnd(value);
        return false;
    }

    private StringIterator GetStaticPartIterator()
    {
        if (!CurrentPartIsStatic(out var iterator))
            throw new InvalidOperationException("Current part is not static");
        return iterator;
    }

    private bool CurrentPartIsStatic([MaybeNullWhen(false)] out StringIterator iterator)
    {
        iterator = null;
        var part = _source[_index];
        if (part is PatternPart.Static)
        {
            iterator = _currentIterator ?? throw new InvalidOperationException("The current iterator is null");
            return true;
        }

        if (part is PatternPart.Dynamic)
        {
            return false;
        }

        throw new UnsupportedInstanceType(part);
    }

    public bool HasNext()
    {
        if (_source.Count < 1)
            return false;

        if (_index < _lastIndex)
            return true;

        if (CurrentPartIsStatic(out var iterator))
        {
            return iterator.HasNext();
        }

        return false;
    }


    public PatternPartsIterator Clone() => new(this);

    public void SavePopPosition()
    {
        if (CurrentPartIsStatic(out var iterator))
        {
            _lastPopIndex = _index;
            _lastPopIndexInPart = iterator.CurrentIndex - 1;
        }
        else
        {
            _lastPopIndex = _index - 1;
        }
    }

    public override string ToString()
    {
        if (_index == -1)
        {
            return ">><<" + string.Concat(_source.Select(x => x.Value));
        }

        string current = _currentIterator != null ? _currentIterator.ToString() : _source[_index].ToString();
        string before = string.Concat(_source.Take(new Range(start: 0, _index)).Select(x => x.ToString()));

        string after = _index == _lastIndex ? "" : string.Concat(_source.Skip(_index + 1));

        var result = $"{before}>>{current}<<{after}";
        return result;
    }
}