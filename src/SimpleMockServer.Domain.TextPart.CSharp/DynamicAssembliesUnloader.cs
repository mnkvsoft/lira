using Microsoft.Extensions.Logging;

// ReSharper disable RedundantExplicitArrayCreation

namespace SimpleMockServer.Domain.TextPart.CSharp;

internal class DynamicAssembliesUnloader
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

        while (_toUnload.Count > 1)
        {
            var (revision, contextToUnload) = _toUnload.Peek();
            try
            {
                contextToUnload.Unload();
                _logger.LogInformation($"Dynamic assemblies with revision {revision} was unloaded");
                _toUnload.Dequeue();
            }
            catch (Exception e)
            {
                _logger.LogInformation(e, $"An error occured while upload dynamic assemblies with revision: {revision}");
                break;
            }
        }
        
        GC.Collect();
    }
}
