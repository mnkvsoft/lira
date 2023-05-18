using SimpleMockServer.Domain.TextPart;

namespace SimpleMockServer.Domain.Configuration.Rules.ValuePatternParsing;

record TransformPipeline(IObjectTextPart ObjectTextPart) : TransformPipelineBase, IObjectTextPart
{
    public override object? Get(RequestData request) => ExecutePipeline(ObjectTextPart.Get(request));
}

record GlobalTransformPipeline(IGlobalObjectTextPart GlobalObjectTextPart) : TransformPipelineBase, IGlobalObjectTextPart
{
    public override object? Get(RequestData request) => Get();
    public object? Get() =>ExecutePipeline(GlobalObjectTextPart.Get());
}

abstract record TransformPipelineBase : IObjectTextPart
{
    private readonly List<ITransformFunction> _transformFunctions = new();
        
    protected object? ExecutePipeline(object? startValue)
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

    public abstract object? Get(RequestData request);
}
