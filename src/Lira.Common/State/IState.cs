namespace Lira.Common.State;

public interface IState
{
    string StateId { get; }
    string Value { get; }
    void Seal();
}