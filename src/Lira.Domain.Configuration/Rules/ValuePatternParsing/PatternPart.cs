using System.Collections;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing;

abstract class PatternPart
{
    public string Value { get; }

    private PatternPart(string value) => Value = value;

    public class Static : PatternPart
    {
        public Static(string value) : base(value)
        {
            if(string.IsNullOrEmpty(value))
                throw new ArgumentException("Value cannot be null or empty", nameof(value));
        }

        public override string ToString() => Value;
    }

    public class Dynamic(string value) : PatternPart(value)
    {
        public override string ToString() => "{{" + Value + "}}";
    }
}

record PatternParts : IReadOnlyList<PatternPart>
{
    private readonly IReadOnlyList<PatternPart> _parts;

    public PatternParts(IReadOnlyList<PatternPart> parts) => _parts = parts;

    public IEnumerator<PatternPart> GetEnumerator() => _parts.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => _parts.Count;

    public PatternPart this[int index] => _parts[index];

    public override string ToString()
    {
        return string.Concat(_parts.Select(x => x.ToString()));
    }
}