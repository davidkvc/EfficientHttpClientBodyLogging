using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfficientHttpClientBodyLogging;

public static class HttpContentWrapper
{
    public static HttpContent WrapRequestContentForLogging(HttpContent content, HttpLoggingOptions loggingOptions, ILogger logger)
    {
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
                //TODO: copy headers
                return loggingContent;
            }
        }

        return content;
    }

    public static HttpContent WrapResponseContentForLogging(HttpContent content, HttpLoggingOptions loggingOptions, ILogger logger)
    {
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
                //TODO: copy headers
                return loggingContent;
            }
        }

        return content;
    }

    public static async Task<Stream> PrepareResponseReadStream(HttpContent content, HttpLoggingOptions loggingOptions, ILogger logger)
    {
        var responseStream = await content.ReadAsStreamAsync();

        var respContentType = content.Headers.ContentType;

        if (respContentType == null)
        {
            return responseStream;
        }

        if (!MediaTypeHeaderValue.TryParse(respContentType.ToString(), out var mediaType))
        {
            return responseStream;
        }

        foreach (var supportedContentType in loggingOptions.BodyContentTypeAllowlist)
        {
            if (supportedContentType.MatchesMediaType(mediaType.MediaType))
            {
                var encoding = mediaType.Encoding ?? supportedContentType.Encoding ?? Encoding.UTF8;
                var responseLoggingStream = new LoggingStream(responseStream, encoding, loggingOptions.ResponseBodyLogLimit, LoggingStream.Content.ResponseBody, logger);
                return responseLoggingStream;
            }
        }

        return responseStream;
    }
}
