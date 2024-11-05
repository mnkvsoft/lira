using System.Dynamic;

namespace Lira.Domain.TextPart.Impl.CSharp.DynamicModel;

// ReSharper disable once UnusedType.Global
public class Bag : DynamicObject
{
    private readonly bool _readOnly;
    private readonly IDictionary<string, object?> _items;

    public Bag(RuleExecutingContext context, bool readOnly)
    {
        _readOnly = readOnly;
        _items = context.Items;
    }

    public override bool TrySetMember(SetMemberBinder binder, object? value)
    {
        if(_readOnly)
            throw new InvalidOperationException("Cannot set property on a read-only object.");

        var name = binder.Name;
        if (string.IsNullOrEmpty(name))
            throw new Exception("Name is empty");

        _items.Add("bag_" + name, value);

        return true;
    }

    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        var name = binder.Name;
        if (string.IsNullOrEmpty(name))
            throw new Exception("Name is empty");

        if (!_items.TryGetValue("bag_" + name, out result))
            throw new Exception($"Bag item '{name}' not existing");

        return true;
    }
}