using Lira.Common.Extensions;

namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create;
internal class RandomCustom : WithArgumentFunction<string[]>, IObjectTextPart, IWithContext
{
    public override string Name => "random";
    private IReadOnlyList<IObjectTextPart> _parts = null!;
    private SystemFunctionContext _context = null!;

    public override bool ArgumentIsRequired => true;

    public async IAsyncEnumerable<dynamic?> Get(RuleExecutingContext context)
    {
        var part = _parts.Random();
        await foreach (var value in part.Get(context))
        {
            yield return value;
        }
    }

    public ReturnType? ReturnType { get; private set; }

    public override void SetArgument(string[] arguments)
    {
        if (arguments.Length == 0)
            throw new Exception("Not empty array required for function 'random'");


        var parts = new List<IObjectTextPart>();

        foreach(var arg in arguments)
        {
            parts.Add(_context.DeclaredItemsProvider.ItsAccessToDeclaredItem(arg, out var part)
                ? part
                : new StaticPart(arg));
        }

        var types = parts.Select(x => x.ReturnType).Distinct().ToArray();
        ReturnType = SameTypes(types) ? types.First() : null;
        _parts = parts;
    }

    public void SetContext(SystemFunctionContext context)
    {
        _context = context;
    }

    private bool SameTypes(IReadOnlyCollection<ReturnType?> types)
    {
        var current = types.First();
        foreach (var type in types.Skip(1))
        {
            if (current != type)
                return false;
            current = type;
        }

        return false;
    }
}
