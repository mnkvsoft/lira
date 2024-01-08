namespace Lira.Domain;

public record Delayed<T>(T Value, TimeSpan? Delay)
{
    public async Task ExecuteWithDelayIfNeed(Func<T, Task> execute)
    {
        if (Delay != null)
            await Task.Delay(Delay.Value);
        await execute(Value);
    }
}
