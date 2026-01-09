namespace Lira.Common;

public class ObjectPath
{
    public abstract record Elem
    {
        public record Field(string Name) : Elem;

        public record Array(string Name, int Index) : Elem;
    }

    public IReadOnlyCollection<Elem> Elems { get; }

    private ObjectPath(IReadOnlyCollection<Elem> elems)
    {
        Elems = elems;
    }

    public static ObjectPath Parse(string path) => new(ParseElems(path));

    private static IReadOnlyCollection<Elem> ParseElems(string path)
    {
        var parts = path.Split('.');
        if (parts.Length == 0)
            throw new ArgumentException("Path to json field is empty");

        var elems = new List<Elem>(parts.Length);

        foreach (var part in parts)
        {
            if (string.IsNullOrWhiteSpace(part))
                throw new ArgumentException("Invalid path to json field. Empty accessor. Path: " + path);

            if (part.Contains('['))
            {
                var split = part.Split('[');
                var fieldName = split[0];

                var indexPart = split[1];
                if (!indexPart.EndsWith(']'))
                    throw new ArgumentException(
                        $"Invalid path to json field. Missing ']' in array access: {part}. Path: " + path);

                var indexStr = indexPart[..^1];
                if (!int.TryParse(indexStr, out int index))
                    throw new ArgumentException(
                        $"Invalid path to json field. Invalid index in array access: {indexStr}. Path: " + part);

                if (index < 0)
                    throw new ArgumentException(
                        $"Invalid path to json field. Negative index in array access: {indexStr}. Path: " + part);

                elems.Add(new Elem.Array(fieldName, index));
            }
            else
            {
                elems.Add(new Elem.Field(part));
            }
        }

        return elems;
    }
}
