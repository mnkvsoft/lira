using SimpleMockServer.Domain.TextPart;

namespace SimpleMockServer.Domain.Configuration.Rules.ValuePatternParsing;

record TransformPipeline(IObjectTextPart ObjectTextPart) : IObjectTextPart
{
    private readonly List<ITransformFunction> _transformFunctions = new();
        
    private object? ExecutePipeline(object? startValue)
    {
        if (_transformFunctions.Count == 0)
            return startValue;
            
        object? result = startValue;
        foreach (var function in _transformFunctions)
        {
            result = function.Transform(result);
        }

        return result;
    }

    public void Add(ITransformFunction transform)
    {
        _transformFunctions.Add(transform);
    }

    public object? Get(RequestData request) => ExecutePipeline(ObjectTextPart.Get(request));
}