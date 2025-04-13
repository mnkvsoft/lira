using Microsoft.Extensions.Logging;

// ReSharper disable RedundantExplicitArrayCreation

namespace Lira.Domain.TextPart.Impl.CSharp;

internal class DynamicAssembliesUnloader : IDisposable
{
    private readonly ILogger _logger;
    private readonly Queue<DynamicAssembliesContext> _toUnload = new();

    public DynamicAssembliesUnloader(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(GetType());
    }

    public void UnloadUnused(DynamicAssembliesContext currentContext)
    {
        _toUnload.Enqueue(currentContext);

        Unload();
    }

    private void Unload()
    {
        while (_toUnload.Count != 0)
        {
            var (revision, contextToUnload) = _toUnload.Peek();
            try
            {
                contextToUnload.Unload();
                _logger.LogDebug($"Dynamic assemblies with revision {revision} were unloaded");
                _toUnload.Dequeue();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"An error occured while upload dynamic assemblies with revision: {revision}");
                break;
            }
        }

        GC.Collect();
    }

    public void Dispose()
    {
        Unload();
    }
}
