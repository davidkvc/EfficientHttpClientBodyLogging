using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp;
using System.Net;
using System.Text;

namespace DavidSmth.Extensions.Http.BodyLogging.Tests;

public partial class LoggingTests
{
    [Fact]
    public async Task Small_request_and_response_body_is_properly_logged()
    {
        var logger = new TestLogger();

        await Execute(logger,
            "hello",
            "world");

        logger.Messages.Should().BeEquivalentTo(
            "RequestBody: hello",
            "ResponseBody: world");
    }

    [Fact]
    public async Task Big_request_and_response_body_is_properly_logged()
    {
        var bigContentSize = 6000;

        var logger = new TestLogger();

        var bigRequestString = new string('a', bigContentSize);
        var bigResponseString = new string('b', bigContentSize);

        await Execute(logger,
            bigRequestString,
            bigResponseString);

        logger.Messages.Should().BeEquivalentTo(
            $"RequestBody: {bigRequestString}",
            $"ResponseBody: {bigResponseString}");
    }

    [Fact]
    public async Task Too_big_request_and_response_body_is_trimmed_in_logs()
    {
        //TODO: convert this into 2 tests to verify that both request and response logging respects it's specific limit
        var tooBigContentSize = 11_000;

        var logger = new TestLogger();

        var tooBigRequestString = new string('a', tooBigContentSize);
        var tooBigResponseString = new string('b', tooBigContentSize);

        await Execute(logger,
            tooBigRequestString,
            tooBigResponseString,
            opts =>
            {
                opts.RequestBodyLogLimit = 10_000;
                opts.ResponseBodyLogLimit = 10_000;
            });

        logger.Messages.Should().BeEquivalentTo(
            $"RequestBody: {new string('a', 10_000)}[Truncated by RequestBodyLogLimit]",
            $"ResponseBody: {new string('b', 10_000)}[Truncated by ResponseBodyLogLimit]");
    }

    [Fact]
    public async Task Logging_doesnt_break_when_sending_or_receiving_null_content()
    {
        var logger = new TestLogger();

        var options = new HttpClientBodyLoggingOptions();

        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Get, "http://logging-example/")
            .Respond(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = null });
        using var client = new HttpClient(new HttpBodyLoggingHandler(Options.Create(options), logger)
        {
            InnerHandler = mockHttp,
        });

        using var msg = new HttpRequestMessage(HttpMethod.Get, "http://logging-example/");
        msg.Content = null;

        using var resp = await client.SendAsync(msg);
        resp.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Logging_doesnt_break_when_sending_or_receiving_empty_content()
    {
        var logger = new TestLogger();

        var options = new HttpClientBodyLoggingOptions();

        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Get, "http://logging-example/")
            .Respond(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(new byte[0]) });
        using var client = new HttpClient(new HttpBodyLoggingHandler(Options.Create(options), logger)
        {
            InnerHandler = mockHttp,
        });

        using var msg = new HttpRequestMessage(HttpMethod.Get, "http://logging-example/");
        msg.Content = new ByteArrayContent(new byte[0]);

        using var resp = await client.SendAsync(msg);
        resp.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Request_logging_for_all_requests_can_be_disabled()
    {
        var logger = new TestLogger();

        await Execute(logger,
            "request",
            "response",
            opts =>
            {
                opts.RequestBodyLogLimit = 0;
            });

        logger.Messages.Should().BeEquivalentTo($"ResponseBody: response");
    }

    [Fact]
    public async Task Response_logging_for_all_requests_can_be_disabled()
    {
        var logger = new TestLogger();

        await Execute(logger,
            "request",
            "response",
            opts =>
            {
                opts.ResponseBodyLogLimit = 0;
            });

        logger.Messages.Should().BeEquivalentTo($"RequestBody: request");
    }

    //TODO: verify disabling certain content types (related to above tests)

    [Fact]
    public void Sync_requests_can_be_logged()
    {
        var logger = new TestLogger();
        var requestBody = "request";

        var options = new HttpClientBodyLoggingOptions();

        var mockHttp = new SimpleMockHttpMessageHandler
        {
            SyncHandler = req =>
            {
                req.ConsumeContent();

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("response", Encoding.UTF8, "text/plain")
                };
            }
        };
        using var client = new HttpClient(new HttpBodyLoggingHandler(Options.Create(options), logger)
        {
            InnerHandler = mockHttp,
        });

        using var msg = new HttpRequestMessage(HttpMethod.Post, "http://logging-example/");
        msg.Content = new StringContent(requestBody, Encoding.UTF8, "text/plain");

        using var resp = client.Send(msg);
        resp.EnsureSuccessStatusCode();
    }

    //TODO: verify more content types, not just JSON
    
    [Fact]
    public async Task Headers_are_preserved_when_logging_is_enabled()
    {
        var logger = new TestLogger();

        var requestBody = "request";
        var responseBody = "response";

        var options = new HttpClientBodyLoggingOptions();

        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Post, "http://logging-example/")
            .WithContent(requestBody) //we need to check the content to force mock handler to actually read it
            .WithHeaders("Content-Type", "text/plain; charset=utf-8")
            .WithHeaders("X-Example", "example")
            .Respond("text/plain", responseBody);
        using var client = new HttpClient(new HttpBodyLoggingHandler(Options.Create(options), logger)
        {
            InnerHandler = mockHttp,
        });

        using var msg = new HttpRequestMessage(HttpMethod.Post, "http://logging-example/");
        msg.Content = new StringContent(requestBody, Encoding.UTF8, "text/plain");
        msg.Content.Headers.Add("X-Example", "example");

        using var resp = await client.SendAsync(msg);
        resp.EnsureSuccessStatusCode();
    }

    private async Task Execute(TestLogger logger, string requestBody, string responseBody, Action<HttpClientBodyLoggingOptions>? configureOptions = null)
    {
        var options = new HttpClientBodyLoggingOptions();
        configureOptions?.Invoke(options);

        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Post, "http://logging-example/")
            .WithContent(requestBody) //we need to check the content to force mock handler to actually read it
            .Respond("text/plain", responseBody);
        using var client = new HttpClient(new HttpBodyLoggingHandler(Options.Create(options), logger)
        {
            InnerHandler = mockHttp,
        });

        using var msg = new HttpRequestMessage(HttpMethod.Post, "http://logging-example/");
        msg.Content = new StringContent(requestBody, Encoding.UTF8, "text/plain");

        using var resp = await client.SendAsync(msg);
        resp.EnsureSuccessStatusCode();
    }
}