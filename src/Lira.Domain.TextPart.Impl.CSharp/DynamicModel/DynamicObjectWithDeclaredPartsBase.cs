// ReSharper disable UnusedMember.Global
namespace Lira.Domain.TextPart.Impl.CSharp.DynamicModel;

public class DynamicObjectWithDeclaredPartsBase : DynamicObjectBase
{
    protected DynamicObjectWithDeclaredPartsBase(DependenciesBase dependencies) : base(dependencies){}

    protected Task<dynamic?> GetDeclaredPart(string name, RuleExecutingContext context)
    {
        return DeclaredItemsProvider.Get(name).Generate(context);
    }

    protected IObjectTextPart GetDeclaredPart(string name)
    {
        return DeclaredItemsProvider.Get(name);
    }

    protected async IAsyncEnumerable<dynamic?> Repeat(RuleExecutingContext context, IObjectTextPart part, string separator, int count)
    {
        for (var i = 0; i < count; i++)
        {
            await foreach (var obj in part.Get(context))
            {
                if(i > 0)
                    yield return separator;
                yield return obj;
            }
        }
    }

    private class RepeatPart(IObjectTextPart part, string separator, int count) : IObjectTextPart
    {
        public async IAsyncEnumerable<dynamic?> Get(RuleExecutingContext context)
        {
            for (var i = 0; i < count; i++)
            {
                await foreach (var obj in part.Get(context))
                {
                    if(i > 0)
                        yield return separator;
                    yield return obj;
                }
            }
        }

        public ReturnType? ReturnType => part.ReturnType;
    }
}