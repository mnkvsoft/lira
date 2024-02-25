using Microsoft.Extensions.Logging;

namespace Lira.Domain.Actions;

public class ActionsExecutor
{
    private readonly IReadOnlyCollection<Delayed<IAction>> _actions;
    private readonly ILogger _logger;


    public ActionsExecutor(IReadOnlyCollection<Delayed<IAction>> actions, ILoggerFactory loggerFactory)
    {
        _actions = actions;
        _logger = loggerFactory.CreateLogger(GetType());
    }

    private async Task TryCall(Delayed<IAction> action, RuleExecutingContext context)
    {
        if (action.Delay != null)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(action.Delay.Value);
                try
                {
                    await action.Value.Execute(context);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"An error occurred while calling '{action.GetType().Name}'");
                }
            });
        }
        else
        {
            await action.Value.Execute(context);    
        }
    }

    public async Task Execute(RuleExecutingContext context)
    {
        await Task.WhenAll(_actions.Select(caller => TryCall(caller, context)));
    }
}