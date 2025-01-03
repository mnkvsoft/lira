using System.Dynamic;
using Lira.Common.Extensions;

namespace Lira.Domain.TextPart.Impl.CSharp.DynamicModel;

public class VariablesWriter
{
    private readonly RuleExecutingContext _context;
    private readonly IDeclaredPartsProvider _declaredPartsProvider;
    private readonly bool _readOnly;

    public VariablesWriter(RuleExecutingContext context, IDeclaredPartsProvider declaredPartsProvider, bool readOnly)
    {
        _context = context;
        _declaredPartsProvider = declaredPartsProvider;
        _readOnly = readOnly;
    }

    public dynamic this[string name]
    {
        get
        {
            throw new Exception("Read not supported");
        }
        set
        {
            if(_readOnly)
                throw new InvalidOperationException("Cannot set property on a read-only object.");

            if (string.IsNullOrEmpty(name))
                throw new Exception("Name is empty");

            _declaredPartsProvider.SetVariable(name, _context, value);
        }
    }
}

// ReSharper disable once UnusedType.Global
public class Bag : DynamicObject
{
    private readonly bool _readOnly;
    private readonly RuleExecutingContext _context;
    private static readonly Type BagKey = typeof(Bag);

    public Bag(RuleExecutingContext context, bool readOnly)
    {
        _readOnly = readOnly;
        _context = context;
    }

    public override bool TrySetMember(SetMemberBinder binder, object? value)
    {
        if(_readOnly)
            throw new InvalidOperationException("Cannot set property on a read-only object.");

        var name = binder.Name;
        if (string.IsNullOrEmpty(name))
            throw new Exception("Name is empty");

        var bagItems = _context.Items.GetOrCreate(BagKey, () => new Dictionary<string, object?>());

        bagItems.Add(name, value);

        return true;
    }

    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        var name = binder.Name;
        if (string.IsNullOrEmpty(name))
            throw new Exception("Name is empty");

        if (!_context.Items.TryGetValue(BagKey, out var bagItemsObj))
            throw new Exception($"Bag item '{name}' not existing");

        var bagItems = (Dictionary<string, object?>)bagItemsObj;

        if (!bagItems.TryGetValue(name, out result))
            throw new Exception($"Bag item '{name}' not existing");

        return true;
    }
}