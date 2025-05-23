using Lira.Common.Extensions;

namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create;
internal class RandomCustom : WithArgumentFunction<string[]>, IObjectTextPart, IWithContext
{
    public override string Name => "random";
    private IReadOnlyList<Func<RuleExecutingContext, Task<dynamic?>>> _factories = null!;
    private SystemFunctionContext _context = null!;

    public override bool ArgumentIsRequired => true;

    public async Task<dynamic?> Get(RuleExecutingContext context)
    {
        var func = _factories.Random();
        var result = await func(context);
        return result;
    }

    public ReturnType? ReturnType { get; private set; }

    public override void SetArgument(string[] arguments)
    {
        if (arguments.Length == 0)
            throw new Exception("Not empty array required for function 'random'");


        var factories = new List<Func<RuleExecutingContext, Task<dynamic?>>>();
        var types = new List<ReturnType?>();

        foreach(var arg in arguments)
        {
            if (_context.DeclaredItemsProvider.ItsAccessToDeclaredItem(arg, out var part))
            {
                factories.Add(ctx => part.Get(ctx));
                types.Add(part.ReturnType);
            }
            else
            {
                types.Add(ReturnType.String);
                factories.Add(_ => Task.FromResult<dynamic?>(arg));
            }
        }

        ReturnType = SameTypes(types) ? types.First() : null;
        _factories = factories;
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
