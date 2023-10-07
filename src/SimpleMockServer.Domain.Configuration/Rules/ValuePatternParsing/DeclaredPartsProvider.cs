using SimpleMockServer.Common.Extensions;
using SimpleMockServer.Domain.TextPart;
using SimpleMockServer.Domain.TextPart.Impl.CSharp;
using SimpleMockServer.Domain.TextPart.Impl.Custom;
using SimpleMockServer.Domain.TextPart.Impl.Custom.FunctionModel;
using SimpleMockServer.Domain.TextPart.Impl.Custom.VariableModel;

namespace SimpleMockServer.Domain.Configuration.Rules.ValuePatternParsing;

class DeclaredPartsProvider : IDeclaredPartsProvider
{
    private readonly IReadonlyDeclaredItems _items;

    public DeclaredPartsProvider(IReadonlyDeclaredItems items)
    {
        _items = items;
    }

    public IObjectTextPart Get(string name)
    {
        if (name.StartsWith(Consts.ControlChars.VariablePrefix))
            return _items.Variables.GetOrThrow(name.TrimStart(Consts.ControlChars.VariablePrefix));
        
        if (name.StartsWith(Consts.ControlChars.FunctionPrefix))
            return _items.Functions.GetOrThrow(name.TrimStart(Consts.ControlChars.FunctionPrefix));
        
        throw new Exception($"Unknown declaration '{name}'");
    }

    public IReadOnlyCollection<string> GetAllNamesDeclared()
    {
        return _items.Variables
                .Select(v => Consts.ControlChars.VariablePrefix + v.Name)
                .Union(_items.Functions.Select(f => Consts.ControlChars.FunctionPrefix + f.Name))
                .ToArray();
    }

    // public bool IsAllowInName(char c) => CustomItemName.IsAllowedCharInName(c);
    //
    // public bool IsStartPart(char c) => c is Consts.ControlChars.VariablePrefix or Consts.ControlChars.FunctionPrefix;
}