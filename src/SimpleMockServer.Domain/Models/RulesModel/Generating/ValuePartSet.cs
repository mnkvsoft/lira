using System.Collections;
using ArgValidation;

namespace SimpleMockServer.Domain.Models.RulesModel.Generating;

public class ValuePartSet : IEnumerable<ValuePart>
{
    private readonly IReadOnlyCollection<ValuePart> _valueParts;

    public ValuePartSet(IReadOnlyCollection<ValuePart> valueParts)
    {
        Arg.NotEmpty(valueParts, nameof(valueParts));
        _valueParts = valueParts;
    }

    public IEnumerator<ValuePart> GetEnumerator()
    {
        return _valueParts.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
