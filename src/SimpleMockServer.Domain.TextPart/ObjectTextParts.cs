using System.Collections;

namespace SimpleMockServer.Domain.TextPart;

// todo: cache result when all static parts
public class ObjectTextParts : IReadOnlyCollection<IObjectTextPart>
{
    private readonly IReadOnlyCollection<IObjectTextPart> _parts;

    public ObjectTextParts(IReadOnlyCollection<IObjectTextPart> valueParts)
    {
        _parts = valueParts;
    }

    public int Count => _parts.Count;

    public IEnumerator<IObjectTextPart> GetEnumerator()
    {
        return _parts.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}


