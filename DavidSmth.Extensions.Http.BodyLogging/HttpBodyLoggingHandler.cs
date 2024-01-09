using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DavidSmth.Extensions.Http.BodyLogging;

/// <summary>
/// An implementation of <see cref="DelegatingHandler"/> that lazily
/// logs request and response body.
/// </summary>
public class HttpBodyLoggingHandler : DelegatingHandler
{
    private readonly HttpClientBodyLoggingOptions _options;
    private readonly ILogger<HttpBodyLoggingHandler> _logger;

    /// <summary>
    /// Creates HttpBodyLoggingHandler with provided options and logger
    /// </summary>
    public HttpBodyLoggingHandler(IOptions<HttpClientBodyLoggingOptions> options, ILogger<HttpBodyLoggingHandler> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Content != null)
        {
            request.Content = HttpContentWrapper.WrapRequestContentForLogging(request.Content, _options, _logger);
        }

        var resp = base.Send(request, cancellationToken);

        if (resp.Content != null)
        {
            resp.Content = HttpContentWrapper.WrapResponseContentForLogging(resp.Content, _options, _logger);
        }

        return resp;
    }

    /// <inheritdoc/>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Content != null)
        {
            request.Content = HttpContentWrapper.WrapRequestContentForLogging(request.Content, _options, _logger);
        }

        var resp = await base.SendAsync(request, cancellationToken);

        if (resp.Content != null)
        {
            resp.Content = HttpContentWrapper.WrapResponseContentForLogging(resp.Content, _options, _logger);
        }

        return resp;
    }
}
