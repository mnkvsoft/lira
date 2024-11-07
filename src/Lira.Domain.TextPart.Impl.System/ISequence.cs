namespace Lira.Domain.TextPart.Impl.System;

public class Sequence: IWithState
{
    private static long _counter;
    private bool _sealed = false;

    public long Next()
    {
        if (_sealed)
            throw new InvalidOperationException("Sequence already sealed");

        return Interlocked.Increment(ref _counter);
    }

    public string StateId => "seq";
    public void RestoreState(string value)
    {
        _counter = long.Parse(value);
    }

    public string GetState()
    {
        return _counter.ToString();
    }

    public void Seal()
    {
        _sealed = true;
    }
}