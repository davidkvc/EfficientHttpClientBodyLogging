
using BenchmarkDotNet.Running;
using DavidSmth.Extensions.Http.BodyLogging;
using DavidSmth.Extensions.Http.BodyLogging.Benchmarks;
using Microsoft.Extensions.Logging.Abstractions;

var data = new byte[10000];
for (int i = 0; i < data.Length; i++)
{
    data[i] = 65;
}
Stream _baseStream1024 = new MemoryStream(data);
byte[] _target = new byte[10000];

for (int i = 0; i < 2; i++)
{
    _baseStream1024.Position = 0;
    var content = new StreamContent(_baseStream1024);
    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");

    var wrappedContent = HttpContentWrapper.WrapRequestContentForLogging(content, new(), NullLogger.Instance);
    var stream = await wrappedContent.ReadAsStreamAsync();

    await stream.ReadAsync(_target);
}

Console.WriteLine("end");

//BenchmarkRunner.Run<RequestBodyReadingBenchmarks>();