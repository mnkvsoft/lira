namespace Lira.Common.State;

class SequenceState : IState
{
    public Int64Sequence Sequence { get; }

    public string StateId { get; }
    public string Value => Sequence.Value.ToString();

    public void Seal() => Sequence.Seal();

    public SequenceState(Int64Sequence sequence, string stateId)
    {
        Sequence = sequence;
        StateId = stateId;
    }
}