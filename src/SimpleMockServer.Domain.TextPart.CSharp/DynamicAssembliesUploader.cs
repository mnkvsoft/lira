using Microsoft.Extensions.Logging;

// ReSharper disable RedundantExplicitArrayCreation

namespace SimpleMockServer.Domain.TextPart.CSharp;

class DynamicAssembliesUploader
{
    private readonly ILogger _logger;
    private Queue<DynamicAssembliesContext> ToUnload = new();

    public DynamicAssembliesUploader(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(GetType());
    }

    public void UnloadUnused(DynamicAssembliesContext currentContext)
    {
        ToUnload.Enqueue(currentContext);

        while (ToUnload.Count > 1)
        {
            var (revision, contextToUnload) = ToUnload.Peek();
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
