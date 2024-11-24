using Lira.Domain.Configuration.Extensions;
using Lira.Domain.TextPart;
using Lira.Domain.TextPart.Impl.CSharp;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing;

class DeclaredPartsProvider : IDeclaredPartsProvider
{
    private readonly IReadonlyDeclaredItems _items;

    public DeclaredPartsProvider(IReadonlyDeclaredItems items)
    {
        _items = items;
    }

    public IObjectTextPart Get(string name) => _items.Get(name);

    public IReadOnlyCollection<string> GetAllNamesDeclared()
    {
        return _items.Variables
                .Select(v => Consts.ControlChars.VariablePrefix + v.Name)
                .Union(_items.Functions.Select(f => Consts.ControlChars.FunctionPrefix + f.Name))
                .ToArray();
    }
}