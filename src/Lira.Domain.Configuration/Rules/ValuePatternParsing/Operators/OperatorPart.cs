using Lira.Domain.TextPart;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators;

class OperatorPart(OperatorDraft draft) : IObjectTextPart
{
    public static readonly string Prefix = "@";
    public static readonly string End = Prefix + "end";

    public OperatorDraft Draft { get; } = draft;

    private IObjectTextPart? _value;

    public IObjectTextPart Value
    {
        get => _value ?? throw new Exception("Value is not initialized");
        set => _value = value;
    }

    public IAsyncEnumerable<dynamic?> Get(RuleExecutingContext context) => Value.Get(context);

    public ReturnType? ReturnType => Value.ReturnType;
}