using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfficientHttpClientBodyLogging;

public class HttpBodyLoggingHandler : DelegatingHandler
{
    private readonly HttpLoggingOptions _options;
    private readonly ILogger<HttpBodyLoggingHandler> _logger;

    public HttpBodyLoggingHandler(IOptions<HttpLoggingOptions> options, ILogger<HttpBodyLoggingHandler> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

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
