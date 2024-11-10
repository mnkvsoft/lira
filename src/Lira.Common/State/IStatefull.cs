namespace Lira.Common.State;

public interface IStateful
{
    string StateId { get; }
    IState GetState();
    void RestoreState(IState state);
    void RestoreState(string value);
}