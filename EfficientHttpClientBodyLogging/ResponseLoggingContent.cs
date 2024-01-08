using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EfficientHttpClientBodyLogging;

//TODO: response body is going to be logged once response body is read which
//might happen later than request processing itself. So the log might not contain
//info about the request. We should make sure to add basic request information
//like method, path, ... to the log
internal class ResponseLoggingContent : HttpContent
{
    private readonly HttpContent _inner;
    private readonly Encoding _encoding;
    private readonly int _limit;
    private readonly ILogger _logger;

    public ResponseLoggingContent(HttpContent inner, Encoding encoding, int limit, ILogger logger)
    {
        _inner = inner;
        _encoding = encoding;
        _limit = limit;
        _logger = logger;
    }


    protected override bool TryComputeLength(out long length)
    {
        length = 0;
        return false;
    }

    private async Task<LoggingStream> CreateStream(CancellationToken cancellationToken = default)
    {
        return new LoggingStream(await _inner.ReadAsStreamAsync(cancellationToken),
            _encoding,
            _limit,
            LoggingStream.Content.ResponseBody,
            _logger);
    }

    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        using var loggingStream = await CreateStream();

        await loggingStream.CopyToAsync(stream);

        loggingStream.Log();
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
