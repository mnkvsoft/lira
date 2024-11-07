namespace Lira.Domain.TextPart;

public interface IWithState
{
    string StateId { get; }
    void RestoreState(string value);
    string GetState();
    void Seal();
}