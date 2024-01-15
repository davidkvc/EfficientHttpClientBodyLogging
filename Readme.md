# Efficient Request/Response body logging with C#'s HttpClient

[![NuGet Version](https://img.shields.io/nuget/v/DavidKvc.Extensions.Http.BodyLogging?style=flat-square&logo=nuget&label=DavidKvc.Extensions.Http.BodyLogging)](https://www.nuget.org/packages/DavidKvc.Extensions.Http.BodyLogging/)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=davidkvc_EfficientHttpClientBodyLogging&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=davidkvc_EfficientHttpClientBodyLogging)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=davidkvc_EfficientHttpClientBodyLogging&metric=coverage)](https://sonarcloud.io/summary/new_code?id=davidkvc_EfficientHttpClientBodyLogging)

This is an example of **memory-efficient** implementation of req/res body logging for C#'s HttpClient.

* Configure max size of req/resp body that will be read as a string and logged
* Use constant-sized linked list of pooled array segments to store bytes while req/resp is being processed

This is heavily inspired by existing implementation of req/res body logging used in
`Microsoft.AspNetCore.HttpLogging`.

## Usage

```csharp
ServiceCollection services = ...;

services.AddHttpClient("example")
    .AddHttpMessageHandler<HttpBodyLoggingHandler>();
services.Configure<HttpClientBodyLoggingOptions>(opts =>
{
    opts.BodyContentTypeAllowlist.Add(new MediaTypeHeaderValue("multipart/form-data"));
});
services.AddTransient<HttpBodyLoggingHandler>();
```

## Log output

This is output from running the example project

```plain
{"EventId":100,"LogLevel":"Information","Category":"System.Net.Http.HttpClient.example.LogicalHandler","Message":"Start processing HTTP request POST https://httpbin.org/post","State":{"Message":"Start processing HTTP request POST https://httpbin.org/post","HttpMethod":"POST","Uri":"https://httpbin.org/post","{OriginalFormat}":"Start processing HTTP request {HttpMethod} {Uri}"}}
{"EventId":100,"LogLevel":"Information","Category":"System.Net.Http.HttpClient.example.ClientHandler","Message":"Sending HTTP request POST https://httpbin.org/post","State":{"Message":"Sending HTTP request POST https://httpbin.org/post","HttpMethod":"POST","Uri":"https://httpbin.org/post","{OriginalFormat}":"Sending HTTP request {HttpMethod} {Uri}"}}
{"EventId":3,"LogLevel":"Information","Category":"DavidKvc.Extensions.Http.BodyLogging.HttpBodyLoggingHandler","Message":"RequestBody: {\u0022data\u0022:\u0022hello world\u0022}","State":{"Message":"RequestBody: {\u0022data\u0022:\u0022hello world\u0022}","Body":"{\u0022data\u0022:\u0022hello world\u0022}","Status":"","{OriginalFormat}":"RequestBody: {Body}{Status}"}}
{"EventId":101,"LogLevel":"Information","Category":"System.Net.Http.HttpClient.example.ClientHandler","Message":"Received HTTP response headers after 964.4831ms - 200","State":{"Message":"Received HTTP response headers after 964.4831ms - 200","ElapsedMilliseconds":964.4831,"StatusCode":200,"{OriginalFormat}":"Received HTTP response headers after {ElapsedMilliseconds}ms - {StatusCode}"}}
{"EventId":101,"LogLevel":"Information","Category":"System.Net.Http.HttpClient.example.LogicalHandler","Message":"End processing HTTP request after 981.6009ms - 200","State":{"Message":"End processing HTTP request after 981.6009ms - 200","ElapsedMilliseconds":981.6009,"StatusCode":200,"{OriginalFormat}":"End processing HTTP request after {ElapsedMilliseconds}ms - {StatusCode}"}}
{"EventId":4,"LogLevel":"Information","Category":"DavidKvc.Extensions.Http.BodyLogging.HttpBodyLoggingHandler","Message":"ResponseBody: {\n  \u0022args\u0022: {}, \n  \u0022data\u0022: \u0022{\\\u0022data\\\u0022:\\\u0022hello world\\\u0022}\u0022, \n  \u0022files\u0022: {}, \n  \u0022form\u0022: {}, \n  \u0022headers\u0022: {\n    \u0022Content-Length\u0022: \u002222\u0022, \n    \u0022Content-Type\u0022: \u0022application/json; charset=utf-8\u0022, \n    \u0022Host\u0022: \u0022httpbin.org\u0022, \n    \u0022X-Amzn-Trace-Id\u0022: \u0022Root=1-65a52f38-3eebef110e3e06bf66735dca\u0022\n  }, \n  \u0022json\u0022: {\n    \u0022data\u0022: \u0022hello world\u0022\n  }, \n  \u0022origin\u0022: \u0022178.40.51.23\u0022, \n  \u0022url\u0022: \u0022https://httpbin.org/post\u0022\n}\n","State":{"Message":"ResponseBody: {\n  \u0022args\u0022: {}, \n  \u0022data\u0022: \u0022{\\\u0022data\\\u0022:\\\u0022hello world\\\u0022}\u0022, \n  \u0022files\u0022: {}, \n  \u0022form\u0022: {}, \n  \u0022headers\u0022: {\n    \u0022Content-Length\u0022: \u002222\u0022, \n    \u0022Content-Type\u0022: \u0022application/json; charset=utf-8\u0022, \n    \u0022Host\u0022: \u0022httpbin.org\u0022, \n    \u0022X-Amzn-Trace-Id\u0022: \u0022Root=1-65a52f38-3eebef110e3e06bf66735dca\u0022\n  }, \n  \u0022json\u0022: {\n    \u0022data\u0022: \u0022hello world\u0022\n  }, \n  \u0022origin\u0022: \u0022178.40.51.23\u0022, \n  \u0022url\u0022: \u0022https://httpbin.org/post\u0022\n}\n","Body":"{\n  \u0022args\u0022: {}, \n  \u0022data\u0022: \u0022{\\\u0022data\\\u0022:\\\u0022hello world\\\u0022}\u0022, \n  \u0022files\u0022: {}, \n  \u0022form\u0022: {}, \n  \u0022headers\u0022: {\n    \u0022Content-Length\u0022: \u002222\u0022, \n    \u0022Content-Type\u0022: \u0022application/json; charset=utf-8\u0022, \n    \u0022Host\u0022: \u0022httpbin.org\u0022, \n    \u0022X-Amzn-Trace-Id\u0022: \u0022Root=1-65a52f38-3eebef110e3e06bf66735dca\u0022\n  }, \n  \u0022json\u0022: {\n    \u0022data\u0022: \u0022hello world\u0022\n  }, \n  \u0022origin\u0022: \u0022178.40.51.23\u0022, \n  \u0022url\u0022: \u0022https://httpbin.org/post\u0022\n}\n","Status":"","{OriginalFormat}":"ResponseBody: {Body}{Status}"}}
```

## Implementation details

Compared to other examples on the internet this implementation doesn't try to read request or response
body outside of standard req/res processing pipeline. If we wanted to first read the body from arbitrary
`HttpContent` we would have to read the entire content into memory which is inefficient, especially if
we only need part of the content.

Instead this implementation provides the `LoggingStream`, and we stream req/res body through that. As
the request is processed `LoggingStream` captures slice of the processed bytes up to configured limit
and eventually converts that to a string efficiently. Only then we log the captured string.

## Benchmarks

WIP

Logging disabled:

| Method         | Mean        | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|--------------- |------------:|---------:|---------:|-------:|-------:|----------:|
| Baseline_5     |    52.58 ns | 0.851 ns | 0.755 ns | 0.0312 |      - |     392 B |
| Baseline_1024  |    62.69 ns | 0.294 ns | 0.261 ns | 0.0312 |      - |     392 B |
| Baseline_10000 |   113.90 ns | 0.372 ns | 0.330 ns | 0.0312 |      - |     392 B |
| Wrapped_5      |   806.35 ns | 4.024 ns | 3.567 ns | 0.1926 | 0.0010 |    2424 B |
| Wrapped_1024   |   886.58 ns | 2.227 ns | 1.738 ns | 0.2537 | 0.0010 |    3192 B |
| Wrapped_10000  | 1,731.81 ns | 9.764 ns | 9.133 ns | 0.9842 | 0.0381 |   12344 B |

Logging enabled:

| Method         | Mean        | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|--------------- |------------:|----------:|----------:|-------:|-------:|----------:|
| Baseline_5     |    53.76 ns |  1.014 ns |  0.899 ns | 0.0312 |      - |     392 B |
| Baseline_1024  |    62.13 ns |  0.385 ns |  0.360 ns | 0.0312 |      - |     392 B |
| Baseline_10000 |   111.99 ns |  0.747 ns |  0.698 ns | 0.0312 |      - |     392 B |
| Wrapped_5      |   882.62 ns |  7.012 ns |  6.559 ns | 0.2451 | 0.0010 |    3080 B |
| Wrapped_1024   | 1,117.03 ns | 12.310 ns | 10.913 ns | 0.5913 | 0.0019 |    7424 B |
| Wrapped_10000  | 4,961.28 ns | 37.148 ns | 32.931 ns | 7.1411 | 1.1826 |   89872 B |