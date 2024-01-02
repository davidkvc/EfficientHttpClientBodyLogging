using Microsoft.Extensions.Logging;

namespace EfficientHttpClientBodyLogging;

internal static partial class HttpLoggingExtensions
{
    [LoggerMessage(3, LogLevel.Information, "RequestBody: {Body}{Status}", EventName = "RequestBody")]
    public static partial void RequestBody(this ILogger logger, string body, string status);

    [LoggerMessage(4, LogLevel.Information, "ResponseBody: {Body}{Status}", EventName = "ResponseBody")]
    public static partial void ResponseBody(this ILogger logger, string body, string status);

    [LoggerMessage(5, LogLevel.Debug, "Decode failure while converting body.", EventName = "DecodeFailure")]
    public static partial void DecodeFailure(this ILogger logger, Exception ex);

    [LoggerMessage(6, LogLevel.Debug, "Unrecognized Content-Type for {Name} body.", EventName = "UnrecognizedMediaType")]
    public static partial void UnrecognizedMediaType(this ILogger logger, string name);

    [LoggerMessage(7, LogLevel.Debug, "No Content-Type header for {Name} body.", EventName = "NoMediaType")]
    public static partial void NoMediaType(this ILogger logger, string name);

    [LoggerMessage(8, LogLevel.Information, "Duration: {Duration}ms", EventName = "Duration")]
    public static partial void Duration(this ILogger logger, double duration);
}
