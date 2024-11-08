namespace Lira.Domain.TextPart;

public interface IState
{
    string StateId { get; }
    void RestoreState(string value);
    string GetState();
    void Seal();
}