using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DavidKvc.Extensions.Http.BodyLogging;

internal class ResponseLoggingContent : HttpContent
{
    private readonly HttpContent _inner;
    private readonly Encoding _encoding;
    private readonly int _limit;
    private readonly ILogger _logger;
    private readonly BodyLoggingContext _bodyLoggingContext;

    public ResponseLoggingContent(HttpContent inner, Encoding encoding, int limit, ILogger logger,
        BodyLoggingContext bodyLoggingContext)
    {
        _inner = inner;
        _encoding = encoding;
        _limit = limit;
        _logger = logger;
        _bodyLoggingContext = bodyLoggingContext;
    }

    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        using var contentStream = await _inner.ReadAsStreamAsync();
        using var loggingStream = new LoggingStream(contentStream, _encoding, _limit,
            LoggingStream.Content.ResponseBody, _logger, _bodyLoggingContext);

        await loggingStream.CopyToAsync(stream);

        loggingStream.Log();
    }

    protected override void SerializeToStream(Stream stream, TransportContext? context, CancellationToken cancellationToken)
    {
        using var contentStream = _inner.ReadAsStream(cancellationToken);
        using var loggingStream = new LoggingStream(contentStream, _encoding, _limit,
            LoggingStream.Content.ResponseBody, _logger, _bodyLoggingContext);

        loggingStream.CopyTo(stream);

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
