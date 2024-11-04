using System.Dynamic;
using Lira.Common;
using Lira.Common.Extensions;
using Lira.Domain.DataModel;

namespace Lira.Domain.TextPart.Impl.CSharp.DynamicModel;

public class DynamicObjectBase
{
    public record Dependencies(
        IDeclaredPartsProvider DeclaredPartsProvider,
        Cache Cache,
        IRangesProvider RangesProvider);

    protected readonly IDeclaredPartsProvider DeclaredPartsProvider;
    protected readonly Cache _cache;
    protected const string NewLine = Constants.NewLine;
    private readonly IRangesProvider _rangesProvider;

    public DynamicObjectBase(Dependencies dependencies)
    {
        DeclaredPartsProvider = dependencies.DeclaredPartsProvider;
        _cache = dependencies.Cache;
        _rangesProvider = dependencies.RangesProvider;
    }

    public dynamic? GetDeclaredPart(string name, RuleExecutingContext context)
    {
        dynamic? part = DeclaredPartsProvider.Get(name).Get(context);
        return part;
    }

    public IObjectTextPart GetDeclaredPart(string name)
    {
        return DeclaredPartsProvider.Get(name);
    }

    protected string Repeat(RuleExecutingContext context, IObjectTextPart part, string separator, int count)
    {
        return string.Join(separator,
            Enumerable.Repeat("", count)
                .Select(_ => part.Get(context)?.ToString() ?? ""));
    }

    protected DataRange GetRange(string rangeName)
    {
        var (name, nameRange) = rangeName.SplitToTwoPartsRequired("/");
        return _rangesProvider.Get(new DataName(name)).Get(new DataName(nameRange));
    }

    protected class Bag : DynamicObject
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

        // public dynamic? this[string name]
        // {
        //     get
        //     {
        //         if (string.IsNullOrEmpty(name))
        //             throw new Exception("Name is empty");
        //
        //         if(!Context.Items.TryGetValue("bag_" + name, out var item))
        //             throw new Exception($"Bag item '{name}' not existing");
        //
        //         return item;
        //     }
        //     set
        //     {
        //         if (string.IsNullOrEmpty(name))
        //             throw new Exception("Name is empty");
        //
        //         Context.Items.Add("bag_" + name, value);
        //     }
        // }
    }
}