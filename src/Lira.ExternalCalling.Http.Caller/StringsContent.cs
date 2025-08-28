using System.Net;
using System.Text;

public class StringsContent : HttpContent
{
    private readonly IEnumerable<string> _strings;
    private readonly Encoding _encoding;
    private readonly string _mediaType;

    public StringsContent(IEnumerable<string> strings)
        : this(strings, Encoding.UTF8, "text/plain")
    {
    }

    public StringsContent(IEnumerable<string> strings, Encoding encoding)
        : this(strings, encoding, "text/plain")
    {
    }

    public StringsContent(IEnumerable<string> strings, Encoding encoding, string mediaType)
    {
        _strings = strings ?? throw new ArgumentNullException(nameof(strings));
        _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        _mediaType = mediaType ?? throw new ArgumentNullException(nameof(mediaType));

        Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(_mediaType)
        {
            CharSet = _encoding.WebName
        };
    }

    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        var writer = new StreamWriter(stream, _encoding);

        foreach (var str in _strings)
        {
            await writer.WriteAsync(str).ConfigureAwait(false);
        }

        await writer.FlushAsync().ConfigureAwait(false);
    }

    protected override bool TryComputeLength(out long length)
    {
        // Мы не можем заранее вычислить длину, так как строки могут быть большими или бесконечными
        length = -1;
        return false;
    }
}