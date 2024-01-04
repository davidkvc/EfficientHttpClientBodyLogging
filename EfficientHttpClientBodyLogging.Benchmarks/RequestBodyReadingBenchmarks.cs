using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfficientHttpClientBodyLogging.Benchmarks;

[MemoryDiagnoser]
public class RequestBodyReadingBenchmarks
{
    private readonly Stream _baseStream5;
    private readonly Stream _baseStream1024;
    private readonly Stream _baseStream10000;
    private readonly byte[] _target = new byte[10000];

    public RequestBodyReadingBenchmarks()
    {
        _baseStream5 = new MemoryStream(new byte[] { 60, 60, 60, 60, 60 });
        _baseStream1024 = new MemoryStream(new byte[1024]);
        _baseStream10000 = new MemoryStream(new byte[10000]);
    }

    [Benchmark]
    public async Task Baseline_5()
    {
        _baseStream5.Position = 0;
        var content = new StreamContent(_baseStream5);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");

        var stream = await content.ReadAsStreamAsync();

        await stream.ReadAsync(_target);
    }

    [Benchmark]
    public async Task Baseline_1024()
    {
        _baseStream1024.Position = 0;
        var content = new StreamContent(_baseStream1024);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");

        var stream = await content.ReadAsStreamAsync();

        await stream.ReadAsync(_target);
    }

    [Benchmark]
    public async Task Baseline_10000()
    {
        _baseStream10000.Position = 0;
        var content = new StreamContent(_baseStream10000);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");

        var stream = await content.ReadAsStreamAsync();

        await stream.ReadAsync(_target);
    }

    [Benchmark]
    public async Task Wrapped_5()
    {
        _baseStream5.Position = 0;
        var content = new StreamContent(_baseStream5);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");

        var wrappedContent = HttpContentWrapper.WrapRequestContentForLogging(content, new(), NullLogger.Instance);
        var stream = await wrappedContent.ReadAsStreamAsync();

        await stream.ReadAsync(_target);
    }

    [Benchmark]
    public async Task Wrapped_1024()
    {
        _baseStream1024.Position = 0;
        var content = new StreamContent(_baseStream1024);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");

        var wrappedContent = HttpContentWrapper.WrapRequestContentForLogging(content, new(), NullLogger.Instance);
        var stream = await wrappedContent.ReadAsStreamAsync();

        await stream.ReadAsync(_target);
    }

    [Benchmark]
    public async Task Wrapped_10000()
    {
        _baseStream10000.Position = 0;
        var content = new StreamContent(_baseStream10000);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");

        var wrappedContent = HttpContentWrapper.WrapRequestContentForLogging(content, new(), NullLogger.Instance);
        var stream = await wrappedContent.ReadAsStreamAsync();

        await stream.ReadAsync(_target);
    }
}
