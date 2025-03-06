using System.Collections;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing;

abstract record PatternPart
{
    public record Static(string Value) : PatternPart
    {
        public override string ToString() => Value;
    }

    public record Dynamic(string Value) : PatternPart
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