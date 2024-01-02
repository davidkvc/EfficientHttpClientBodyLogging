using Microsoft.Net.Http.Headers;
using System.Text;

namespace EfficientHttpClientBodyLogging;

public class HttpLoggingOptions
{
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
    /// </summary>
    public int RequestBodyLogLimit { get; set; } = 32 * 1024;

    /// <summary>
    /// Maximum response body size to log (in bytes). Defaults to 32 KB.
    /// </summary>
    public int ResponseBodyLogLimit { get; set; } = 32 * 1024;
}
