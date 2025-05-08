// using System.Collections;
//
// namespace Lira.Common;
//
// public interface IUniqueSetItem
// {
//     string Name { get; }
//     string EntityName { get; }
// }
//
// public abstract class UniqueSet<T> : IReadOnlyCollection<T> where T : IUniqueSetItem
// {
//     private readonly HashSet<T> _set = new();
//
//     protected UniqueSet()
//     {
//     }
//
//     protected UniqueSet(IReadOnlyCollection<T> set)
//     {
//         AddRange(set);
//     }
//
//     public void Add(T item)
//     {
//         if (!_set.Add(item))
//             throw new InvalidOperationException($"{item.EntityName} '{item.Name}' already declared");
//     }
//
//     protected void TryAdd(T item)
//     {
//         _set.Add(item);
//     }
//
//     public void AddRange(IReadOnlyCollection<T> items)
//     {
//         foreach(var item in items)
//         {
//             Add(item);
//         }
//     }
//
//     public void TryAddRange(IReadOnlyCollection<T> items)
//     {
//         foreach(var item in items)
//         {
//             TryAdd(item);
//         }
//     }
//
//     public int Count => _set.Count;
//
//     public IEnumerator<T> GetEnumerator()
//     {
//         return _set.GetEnumerator();
//     }
//
//     IEnumerator IEnumerable.GetEnumerator()
//     {
//         return GetEnumerator();
//     }
// }
