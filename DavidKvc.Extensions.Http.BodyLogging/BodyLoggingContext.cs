using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DavidKvc.Extensions.Http.BodyLogging;

internal class BodyLoggingContext
{
    public static BodyLoggingContext Empty { get; } = new BodyLoggingContext();

    private readonly Dictionary<string, object>? _scope;

    public BodyLoggingContext(HttpResponseMessage response)
    {
        _scope = new Dictionary<string, object>();
        _scope["HttpMethod"] = response.RequestMessage?.Method.ToString() ?? "<unknown>";
        _scope["Uri"] = response.RequestMessage?.RequestUri?.ToString() ?? "<unknown>";
        _scope["StatusCode"] = (int)response.StatusCode;
    }

    private BodyLoggingContext()
    {
            
    }

    public IDisposable? Use(ILogger logger)
    {
        if (_scope == null)
        {
            return null;
        }
        
        return logger.BeginScope(_scope);
    }
}
