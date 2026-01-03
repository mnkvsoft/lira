using System.Diagnostics.CodeAnalysis;
using System.Text;
using Lira.Common.Extensions;
using Microsoft.AspNetCore.Http;

namespace Lira.Domain.Utils;

public static class ContentTypeHeaderUtils
{
    public static readonly Encoding[] AvailableEncodings =
        Encoding.GetEncodings()
        .Select(x => x.GetEncoding())
        .ToArray();

    public static bool TryGetEncoding(
        string charset,
        [MaybeNullWhen(false)] out Encoding encoding)
    {
        encoding = AvailableEncodings.SingleOrDefault(x => x.WebName.Equals(charset, StringComparison.OrdinalIgnoreCase));
        return encoding != null;
    }

    public static Encoding? TryGetContentEncoding(this HttpResponse response)
    {
        var contentType = (string?)response.ContentType;
        if(contentType == null)
            return null;

        var charset = ExtractCharset(contentType);
        if(charset == null)
            return null;
        TryGetEncoding(charset, out var encoding);
        return encoding;
    }

    internal static string? ExtractCharset(this Header contentTypeHeader)
    {
        if(contentTypeHeader.Name != "Content-Type")
            throw new ArgumentException($"Invalid content type: {contentTypeHeader.Name}");

        return contentTypeHeader.Value == null ? null : ExtractCharset(contentTypeHeader.Value);
    }

    public static string? ExtractCharset(string contentType)
    {
        if(!contentType.Contains("charset"))
            return null;

        return contentType.Split(';')
            .Single(x => x.Contains("charset"))
            .SplitToTwoPartsRequired("=")
            .Second
            .Trim();
    }
}