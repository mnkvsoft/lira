namespace Lira.Common.State;

public class SequenceStateful : IStateful
{
    private Int64Sequence _sequence;

    public SequenceStateful(Int64Sequence sequence, string stateId)
    {
        _sequence = sequence;
        StateId = stateId;
    }

    public string StateId { get; }

    public long Next() => _sequence.Next();

    public Interval<long> Interval => _sequence.Interval;

    public IState GetState()
    {
        return new SequenceState(_sequence, StateId);
    }

    public void RestoreState(string value)
    {
        _sequence.SetValue(long.Parse(value));
    }

    public void RestoreState(IState state)
    {
        if(state is not SequenceState sequenceState)
            throw new Exception($"Cannot restore state of type {state.GetType().Name}");

        if(StateId != state.StateId)
            throw new Exception($"Cannot restore state with different state id: {state.StateId}. Expected: {StateId}");

        _sequence = sequenceState.Sequence;
    }
}