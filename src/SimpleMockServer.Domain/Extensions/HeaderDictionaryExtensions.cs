//namespace SimpleMockServer.Domain.Extensions;

//internal static class HeaderDictionaryExtensions
//{
//    public static string GerRequiredHeader(this IHeaderDictionary headers, string name)
//    {
//        if (!headers.TryGetValue(name, out var values))
//            throw new Exception($"Required header '{name}' not found");

//        if (values.Count != 1)
//            throw new Exception($"Header '{name}' nas no single value: " + string.Join(", ", values.Select(x => $"'{x}'")));

//        string value = values[0];

//        if (string.IsNullOrWhiteSpace(value))
//            throw new Exception($"Header '{name}' is empty");

//        return value;
//    }
//}