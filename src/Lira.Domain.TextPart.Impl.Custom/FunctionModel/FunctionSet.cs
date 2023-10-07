using Lira.Common;

namespace Lira.Domain.TextPart.Impl.Custom;

public class FunctionSet : UniqueSet<Function>
{
    public FunctionSet(IReadOnlyCollection<Function> functions) : base(functions)
    {
    }

    public FunctionSet()
    {
    }
}

// public class DeclaredItemSet : IReadOnlyCollection<DeclaredItem>
// {
//     private readonly UniqueSet<Variable> _variables = new();
//     private readonly UniqueSet<Function> _functions = new();
//
//     public DeclaredItemSet()
//     {
//     }
//
//     public DeclaredItemSet(IReadOnlyCollection<DeclaredItem> set)
//     {
//         AddRange(set);
//     }
//
//     public void Add(DeclaredItem item)
//     {
//         switch (item)
//         {
//             case Variable variable:
//                 _variables.Add(variable);
//                 return;
//             case Function function:
//                 _functions.Add(function);
//                 return;
//             default:
//                 throw new UnsupportedInstanceType(item);
//         }
//     }
//
//     public void AddRange(IReadOnlyCollection<DeclaredItem> set)
//     {
//         foreach (var item in set)
//         {
//             Add(item);
//         }
//     }
//
//     public IEnumerator<DeclaredItem> GetEnumerator()
//     {
//         return _variables
//             .Cast<DeclaredItem>()
//             .Union(
//                 _functions.Cast<DeclaredItem>())
//             .GetEnumerator();
//     }
//
//     IEnumerator IEnumerable.GetEnumerator()
//     {
//         return GetEnumerator();
//     }
//
//     public int Count => _variables.Count + _functions.Count;
// }
