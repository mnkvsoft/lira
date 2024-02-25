//using System.Collections;

//namespace Lira.Domain;

//public record MatchedValue(string Id, string? Value) : IEquatable<MatchedValue>
//{
//    public override 
//}

//public class MatchedValuesSet : IEnumerable<KeyValuePair<string, string?>>
//{
//    private List<KeyValuePair<string, string?>>? _values;
//    private bool _isReadOnly = false;

//    public static readonly MatchedValuesSet Empty = new MatchedValuesSet
//    {
//        _isReadOnly = true
//    };

//    public void Add(string id, string? value)
//    {
//        if (_isReadOnly)
//            throw new InvalidOperationException("Cannot add value in readonly mode");

//        if (_values == null)
//        {
//            _values = new List<KeyValuePair<string, string?>>
//            {
//                new KeyValuePair<string, string?>(id, value)
//            };
//            return;
//        }

//        if (_values.Any(x => x.Key == id))
//        {
//            throw new ArgumentException($"Value with id '{id}' already added. " +
//                $"Current value: '{value ?? "null"}'. " +
//                $"Added value: '{_values.First(x => x.Key == id).Value}'");
//        }

//        _values.Add(new KeyValuePair<string, string?>(id, value));
//    }

//    public string? Get(string id)
//    {
//        if(_values == null)
//            throw new ArgumentException($"Value with id '{id}' not foundd");

//        if (!_values.Any(x => x.Key == id))
//            throw new ArgumentException($"Value with id '{id}' not foundd");

//        KeyValuePair<string, string?>? found = _values.FirstOrDefault(x => x.Key == id);
//        return found?.Value;
//    }

//    internal void AddFrom(MatchedValuesSet matchedValuesSet)
//    {
//        foreach(var pair in matchedValuesSet)
//        {
//            Add(pair.Key, pair.Value);
//        }
//    }

//    static readonly List<KeyValuePair<string, string?>> empty = new List<KeyValuePair<string, string?>>();
//    public IEnumerator<KeyValuePair<string, string?>> GetEnumerator()
//    {
//        return _values == null ? empty.GetEnumerator() : (IEnumerator<KeyValuePair<string, string?>>)_values.GetEnumerator();
//    }

//    IEnumerator IEnumerable.GetEnumerator()
//    {
//        return GetEnumerator();
//    }
//}
