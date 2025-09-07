using Lira.Domain.TextPart;

namespace Lira.Domain.Configuration;

interface IRulesStorageStrategy : IRulesPathProvider
{
    event Func<Task> OnChanged;
    void InitIfNeed();
}