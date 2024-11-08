using Lira.Common;
using Lira.Domain.TextPart;

namespace Lira.Domain.DataModel;

class SequenceState(Int64Sequence sequence, string stateId) : IState
{
    public string StateId { get; } = stateId;

    public void RestoreState(string value)
    {
        sequence.SetValue(long.Parse(value));
    }

    public string GetState() => sequence.Value.ToString();

    public void Seal() => sequence.Seal();
}