namespace Lira.Domain.Configuration;

interface IRulesStorageStrategy
{
    string Path { get; }
    event Func<Task> OnChanged;
    void InitIfNeed();
}