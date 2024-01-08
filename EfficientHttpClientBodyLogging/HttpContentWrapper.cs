using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfficientHttpClientBodyLogging;

internal static class HttpContentWrapper
{
    public static HttpContent WrapRequestContentForLogging(HttpContent content, HttpClientBodyLoggingOptions loggingOptions, ILogger logger)
    {
        if (loggingOptions.RequestBodyLogLimit == 0)
        {
            return content;
        }

        var reqContentType = content.Headers.ContentType;

        if (reqContentType == null)
        {
            return content;
        }

        if (!MediaTypeHeaderValue.TryParse(reqContentType.ToString(), out var mediaType))
        {
            return content;
        }

        foreach (var supportedContentType in loggingOptions.BodyContentTypeAllowlist)
        {
            if (supportedContentType.MatchesMediaType(mediaType.MediaType))
            {
                var encoding = mediaType.Encoding ?? supportedContentType.Encoding ?? Encoding.UTF8;
                var loggingContent = new RequestLoggingContent(content, encoding, loggingOptions.RequestBodyLogLimit, logger);
                CopyHeaders(content, loggingContent);
                return loggingContent;
            }
        }

        return content;
    }

    public static HttpContent WrapResponseContentForLogging(HttpContent content, HttpClientBodyLoggingOptions loggingOptions, ILogger logger)
    {
        if (loggingOptions.ResponseBodyLogLimit == 0)
        {
            return content;
        }

        var resContentType = content.Headers.ContentType;

        if (resContentType == null)
        {
            return content;
        }

        if (!MediaTypeHeaderValue.TryParse(resContentType.ToString(), out var mediaType))
        {
            return content;
        }

        foreach (var supportedContentType in loggingOptions.BodyContentTypeAllowlist)
        {
            if (supportedContentType.MatchesMediaType(mediaType.MediaType))
            {
                var encoding = mediaType.Encoding ?? supportedContentType.Encoding ?? Encoding.UTF8;
                var loggingContent = new ResponseLoggingContent(content, encoding, loggingOptions.ResponseBodyLogLimit, logger);
                CopyHeaders(content, loggingContent);
                return loggingContent;
            }
        }

        return content;
    }

    private static void CopyHeaders(HttpContent source, HttpContent destination)
    {
        //TODO: implement this
    }
}
