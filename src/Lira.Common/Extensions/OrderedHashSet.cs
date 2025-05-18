using System.Collections;

namespace Lira.Common.Extensions;

public class OrderedHashSet<T> : IList<T>, ISet<T>, IReadOnlyList<T>, IReadOnlySet<T>
{
    private readonly HashSet<T> _hashSet;
    private readonly List<T> _list;
    private readonly IEqualityComparer<T> _comparer;

    public OrderedHashSet() : this(EqualityComparer<T>.Default) { }

    public OrderedHashSet(IEqualityComparer<T> comparer)
    {
        _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
        _hashSet = new HashSet<T>(_comparer);
        _list = new List<T>();
    }

    public OrderedHashSet(IEnumerable<T> collection) : this(collection, EqualityComparer<T>.Default) { }

    public OrderedHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer) : this(comparer)
    {
        if (collection == null) throw new ArgumentNullException(nameof(collection));
        foreach (var item in collection)
        {
            Add(item);
        }
    }

    #region ISet<T> Members

    public bool Add(T item)
    {
        if (_hashSet.Add(item))
        {
            _list.Add(item);
            return true;
        }

        return false;
    }

    public void ExceptWith(IEnumerable<T> other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));
        foreach (var item in other)
        {
            Remove(item);
        }
    }

    public void IntersectWith(IEnumerable<T> other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));
        var otherSet = new HashSet<T>(other, _comparer);
        var itemsToRemove = _hashSet.Where(item => !otherSet.Contains(item)).ToList();
        ExceptWith(itemsToRemove);
    }

    public bool IsProperSubsetOf(IEnumerable<T> other) => _hashSet.IsProperSubsetOf(other);

    public bool IsProperSupersetOf(IEnumerable<T> other) => _hashSet.IsProperSupersetOf(other);

    public bool IsSubsetOf(IEnumerable<T> other) => _hashSet.IsSubsetOf(other);

    public bool IsSupersetOf(IEnumerable<T> other) => _hashSet.IsSupersetOf(other);

    public bool Overlaps(IEnumerable<T> other) => _hashSet.Overlaps(other);

    public bool SetEquals(IEnumerable<T> other) => _hashSet.SetEquals(other);

    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));
        foreach (var item in other)
        {
            if (!Remove(item))
            {
                Add(item);
            }
        }
    }

    public void UnionWith(IEnumerable<T> other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));
        foreach (var item in other)
        {
            Add(item);
        }
    }

    #endregion

    #region IList<T> Members

    public int IndexOf(T item) => _list.IndexOf(item);

    public void Insert(int index, T item)
    {
        if (_hashSet.Add(item))
        {
            _list.Insert(index, item);
        }
        else
        {
            throw new ArgumentException("Item already exists in the set", nameof(item));
        }
    }

    public void RemoveAt(int index)
    {
        var item = _list[index];
        _hashSet.Remove(item);
        _list.RemoveAt(index);
    }

    public T this[int index]
    {
        get => _list[index];
        set
        {
            var existingItem = _list[index];
            if (_comparer.Equals(value, existingItem))
            {
                return; // Same item, no change needed
            }

            if (_hashSet.Add(value))
            {
                _hashSet.Remove(existingItem);
                _list[index] = value;
            }
            else
            {
                throw new ArgumentException("Item already exists in the set", nameof(value));
            }
        }
    }

    #endregion

    #region ICollection<T> Members

    public int Count => _hashSet.Count;

    public bool IsReadOnly => false;

    void ICollection<T>.Add(T item) => Add(item);

    public void Clear()
    {
        _hashSet.Clear();
        _list.Clear();
    }

    public bool Contains(T item) => _hashSet.Contains(item);

    public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

    public bool Remove(T item)
    {
        if (_hashSet.Remove(item))
        {
            _list.Remove(item);
            return true;
        }

        return false;
    }

    #endregion

    #region IEnumerable<T> Members

    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion

    #region Additional Methods

    public void AddRange(IEnumerable<T> items)
    {
        if (items == null) throw new ArgumentNullException(nameof(items));
        foreach (var item in items)
        {
            Add(item);
        }
    }

    public void Sort() => _list.Sort();

    public void Sort(IComparer<T> comparer) => _list.Sort(comparer);

    public void Sort(Comparison<T> comparison) => _list.Sort(comparison);

    public List<T> GetRange(int index, int count) => _list.GetRange(index, count);

    public void Reverse() => _list.Reverse();

    public int BinarySearch(T item) => _list.BinarySearch(item);

    public int BinarySearch(T item, IComparer<T> comparer) => _list.BinarySearch(item, comparer);

    public int BinarySearch(int index, int count, T item, IComparer<T> comparer) =>
        _list.BinarySearch(index, count, item, comparer);

    #endregion
}