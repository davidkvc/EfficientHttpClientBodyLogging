# Efficient Request/Response body logging with C#'s HttpClient

[![NuGet Version](https://img.shields.io/nuget/v/DavidSmth.Extensions.Http.BodyLogging?style=flat-square&logo=nuget&label=DavidSmth.Extensions.Http.BodyLogging)](https://www.nuget.org/packages/DavidSmth.Extensions.Http.BodyLogging/)

This is an example of **memory-efficient** implementation of req/res body logging for C#'s HttpClient.

* Configure max size of req/resp body that will be read as a string and logged
* Use constant-sized linked list of pooled array segments to store bytes while req/resp is being processed

This is heavily inspired by existing implementation of req/res body logging used in
`Microsoft.AspNetCore.HttpLogging`.

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