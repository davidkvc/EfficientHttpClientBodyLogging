using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;

namespace EfficientHttpClientBodyLogging;

internal class RequestLoggingContent : HttpContent
{
    private readonly HttpContent _inner;
    private readonly Encoding _encoding;
    private readonly int _limit;
    private readonly ILogger _logger;

    public RequestLoggingContent(HttpContent inner, Encoding encoding, int limit, ILogger logger)
    {
        _inner = inner;
        _encoding = encoding;
        _limit = limit;
        _logger = logger;
    }

    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        await using var loggingStream = new LoggingStream(stream, _encoding, _limit, LoggingStream.Content.RequestBody, _logger);

        await _inner.CopyToAsync(loggingStream, context);

        loggingStream.Log();
    }

    protected override bool TryComputeLength(out long length)
    {
        length = 0;
        return false;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _inner.Dispose();
        }
        base.Dispose(disposing);
    }
}
