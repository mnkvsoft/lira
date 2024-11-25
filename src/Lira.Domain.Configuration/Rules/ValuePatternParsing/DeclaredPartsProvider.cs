using Lira.Domain.Configuration.Extensions;
using Lira.Domain.TextPart;
using Lira.Domain.TextPart.Impl.CSharp;
using Lira.Common.Extensions;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing;

class DeclaredPartsProvider : IDeclaredPartsProvider
{
    private readonly IReadonlyDeclaredItems _items;

    public DeclaredPartsProvider(IReadonlyDeclaredItems items)
    {
        _items = items;
    }

    public IObjectTextPart Get(string name) => _items.Get(name);

    public ReturnType? GetPartType(string name)
    {
        var variable = _items.Variables
            .FirstOrDefault(v => v.Name == name.TrimStart(Consts.ControlChars.VariablePrefix));

        if (variable != null)
            return variable.Type;

        var function = _items.Functions
            .FirstOrDefault(v => v.Name == name.TrimStart(Consts.ControlChars.FunctionPrefix));

        return function?.Type;
    }
}