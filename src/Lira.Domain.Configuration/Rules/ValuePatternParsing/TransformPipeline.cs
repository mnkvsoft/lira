using Lira.Domain.TextPart;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing;

record TransformPipeline(IObjectTextPart ObjectTextPart) : IObjectTextPart
{
    private readonly List<ITransformFunction> _transformFunctions = new();

    public Type Type
    {
        get
        {
            var type = _transformFunctions.Count == 0
                ? ObjectTextPart.Type
                : _transformFunctions.Last().Type;
            return type;
        }
    }

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

    public dynamic Get(RuleExecutingContext context)
    {
        var value = ObjectTextPart.Get(context);
        dynamic result = ExecutePipeline(value);
        return result;
    }
}