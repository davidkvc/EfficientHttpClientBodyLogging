using Microsoft.Extensions.Logging;

namespace DavidSmth.Extensions.Http.BodyLogging;

internal static partial class HttpLoggingExtensions
{
    [LoggerMessage(3, LogLevel.Information, "RequestBody: {Body}{Status}", EventName = "RequestBody")]
    public static partial void RequestBody(this ILogger logger, string body, string status);

    [LoggerMessage(4, LogLevel.Information, "ResponseBody: {Body}{Status}", EventName = "ResponseBody")]
    public static partial void ResponseBody(this ILogger logger, string body, string status);

    [LoggerMessage(5, LogLevel.Debug, "Decode failure while converting body.", EventName = "DecodeFailure")]
    public static partial void DecodeFailure(this ILogger logger, Exception ex);
}
