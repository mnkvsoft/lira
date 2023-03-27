namespace SimpleMockServer.ConfigurationProviding.Rules;

class LinesIterator
{
    public string[] _lines { get; }
    public int MaxIndex;
    private int _index = -1;
    public bool Next()
    {
        _index++;

        if (_index > MaxIndex)
            return false;

        _currentLine = GetCurrentLine();
        return true;
    }

    private string? _currentLine = null;
    public string CurrentLine
    {
        get
        {
            if (_currentLine == null)
                throw new Exception($"Before get {nameof(CurrentLine)} need invoke {nameof(Next)}() or {nameof(GetNext)}()");

            return _currentLine;
        }
    }
    public string GetNext()
    {
        _index++;

        if (_index > MaxIndex)
            throw new IndexOutOfRangeException($"Max index: {MaxIndex}. Current: {_index}");

        _currentLine = GetCurrentLine();
        return _currentLine;
    }

    private string GetCurrentLine()
    {
        return _lines[_index].Trim();
    }

    public LinesIterator(string[] lines)
    {
        _lines = lines;
        MaxIndex = lines.Length - 1;
    }
}