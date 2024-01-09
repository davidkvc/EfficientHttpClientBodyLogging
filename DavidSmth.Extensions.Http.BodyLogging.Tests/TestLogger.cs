using DavidSmth.Extensions.Http.BodyLogging;
using Microsoft.Extensions.Logging;

namespace DavidSmth.Extensions.Http.BodyLogging.Tests;

public partial class LoggingTests
{
    class TestLogger : ILogger<HttpBodyLoggingHandler>
    {
        public List<(string msg, LogLevel logLevel, EventId eventId, object? state, Exception? exception)> Events = new();
        public IEnumerable<string> Messages => Events.Select(x => x.msg);

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Events.Add((formatter(state, exception), logLevel, eventId, state, exception));
        }
    }


}