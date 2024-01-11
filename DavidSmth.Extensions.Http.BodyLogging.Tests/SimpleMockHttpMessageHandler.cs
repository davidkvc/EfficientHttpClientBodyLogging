namespace DavidSmth.Extensions.Http.BodyLogging.Tests;

public partial class LoggingTests
{
    class SimpleMockHttpMessageHandler : HttpMessageHandler
    {
        public Func<HttpRequestMessage, Task<HttpResponseMessage>> AsyncHandler { get; set; } = null!;
        public Func<HttpRequestMessage, HttpResponseMessage> SyncHandler { get; set; } = null!;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return AsyncHandler(request);
        }

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return SyncHandler(request);
        }
    }
}