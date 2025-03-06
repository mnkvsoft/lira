using System.Collections;

namespace Lira.Domain.TextPart;

public class ObjectTextParts(IReadOnlyCollection<IObjectTextPart> valueParts, bool isString) : IReadOnlyCollection<IObjectTextPart>
{
    public int Count => valueParts.Count;
    public bool IsString => isString;


    public IEnumerator<IObjectTextPart> GetEnumerator()
    {
        return valueParts.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}


