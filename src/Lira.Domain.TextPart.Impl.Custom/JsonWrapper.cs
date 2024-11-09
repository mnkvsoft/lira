using Lira.Domain.TextPart.Types;

namespace Lira.Domain.TextPart.Impl.Custom;

public class JsonWrapper : IObjectTextPart
{
    private readonly ObjectTextParts _parts;
    private readonly string _declaredName;

    public JsonWrapper(ObjectTextParts parts, string declaredName)
    {
        _parts = parts;
        _declaredName = declaredName;
    }

    public async Task<dynamic?> Get(RuleExecutingContext context)
    {
        dynamic? value = await _parts.Generate(context);

        if (value is not string json)
            throw new Exception(GetMessage(value));

        try
        {
            return new Json(json);
        }
        catch (Exception e)
        {
            throw new Exception(GetMessage(value), e);
        }

        string GetMessage(dynamic? o)
        {
            return $"Value in {_declaredName} cannot be convert to json. Value: {o?.ToString()}";
        }
    }
}