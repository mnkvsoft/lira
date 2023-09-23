using System.Globalization;
using SimpleMockServer.Common;
using SimpleMockServer.Domain.Generating;

namespace SimpleMockServer.Domain.TextPart;

public static class ObjectTextPartsExtensions
{
    public static object? Generate(this IReadOnlyCollection<IObjectTextPart> parts, RequestData request)
    {
        return parts.Count == 1 ? parts.First().Get(request) : string.Concat(parts.Select(p => p.Get(request)));
    }
    
    public static TextParts WrapToTextParts(this IReadOnlyCollection<IObjectTextPart> parts)
    {
        return new TextParts(parts.Select(x => new TextPartAdapter(x)).ToArray());
    }


    private record TextPartAdapter(IObjectTextPart ObjectTextPart) : ITextPart
    {
        public string? Get(RequestData request)
        {
            dynamic? obj = ObjectTextPart.Get(request);

            if (obj == null)
                return null;

            if (obj is DateTime date)
                return date.ToString("O");

            return obj.ToString();
        }
    }
}
