using Lira.Domain.TextPart;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing;

record TransformPipeline(IObjectTextPart ObjectTextPart) : IObjectTextPart
{
    private readonly List<ITransformFunction> _transformFunctions = new();

    public ReturnType? ReturnType => _transformFunctions.Count == 0
        ? ObjectTextPart.ReturnType
        : _transformFunctions.Last().ReturnType;

    private dynamic? ExecutePipeline(dynamic? startValue)
    {
        if (_transformFunctions.Count == 0)
            return startValue;

        dynamic? result = startValue;
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

    public async Task<dynamic?> Get(RuleExecutingContext context)
    {
        dynamic? value = await ObjectTextPart.Get(context);
        return ExecutePipeline(value);
    }
}