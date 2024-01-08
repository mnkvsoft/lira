using System.Collections;
using System.Diagnostics;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing;

abstract record PatternPart
{
    [DebuggerDisplay("Value{Name}")]
    public record Static(string Value) : PatternPart
    {
        public override string ToString() => "[[" + Value + "]]";
    }

    [DebuggerDisplay("Value{{{Name}}}")]
    public record Dynamic(string Value) : PatternPart
    {
        public override string ToString() => "{{" + Value + "}}";
    }
}

record PatternParts : IReadOnlyList<PatternPart>
{
    private readonly IReadOnlyList<PatternPart> _parts;

    public PatternParts(IReadOnlyList<PatternPart> parts) => _parts = parts;
    // public PatternParts(IEnumerable<PatternPart> parts) => _parts = new List<PatternPart>(parts);
    public PatternParts(PatternPart part) => _parts = new []{part};

    public IEnumerator<PatternPart> GetEnumerator() => _parts.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => _parts.Count;

    public PatternPart this[int index] => _parts[index];

    public override string ToString()
    {
        return string.Join("", _parts.Select(x => x.ToString()));
    }
}