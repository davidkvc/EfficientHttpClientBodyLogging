using Microsoft.Net.Http.Headers;
using System.Text;

namespace EfficientHttpClientBodyLogging;

/// <summary>
/// Defines options for logging of request and response bodies
/// </summary>
public class HttpClientBodyLoggingOptions
{
    /// <summary>
    /// Request/Response bodies with these content types will be logged.
    /// <para>Wildcard media types are also supported, e.g. text/* or application/*+json</para>
    /// </summary>
    public List<MediaTypeHeaderValue> BodyContentTypeAllowlist { get; } = new List<MediaTypeHeaderValue>
    {
        new("application/json") { Encoding = Encoding.UTF8 },
        new("application/*+json") { Encoding = Encoding.UTF8 },
        new("application/xml") { Encoding = Encoding.UTF8 },
        new("application/*+xml") { Encoding = Encoding.UTF8 },
        new("text/*") { Encoding = Encoding.UTF8 }
    };

    /// <summary>
    /// Maximum request body size to log (in bytes). Defaults to 32 KB.
    /// <para>Set to 0 to disable request body logging</para>
    /// </summary>
    public int RequestBodyLogLimit { get; set; } = 32 * 1024;

    /// <summary>
    /// Maximum response body size to log (in bytes). Defaults to 32 KB.
    /// <para>Set to 0 to disable response body logging</para>
    /// </summary>
    public int ResponseBodyLogLimit { get; set; } = 32 * 1024;
}
