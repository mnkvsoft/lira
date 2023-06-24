using Microsoft.Extensions.Logging;

// ReSharper disable RedundantExplicitArrayCreation

namespace SimpleMockServer.Domain.TextPart.CSharp;

class DynamicAssembliesUploader
{
    private readonly ILogger _logger;
    private readonly Queue<DynamicAssembliesContext> _toUnload = new();

    public DynamicAssembliesUploader(ILoggerFactory loggerFactory)
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
                _logger.LogInformation($"Dynamic assemblies with revision {revision} was uploaded");

            }
            catch (Exception e)
            {
                _logger.LogInformation(e, $"An error occured while upload dynamic assemblies with revision: {revision}");
                break;
            }
        }
    }
}
