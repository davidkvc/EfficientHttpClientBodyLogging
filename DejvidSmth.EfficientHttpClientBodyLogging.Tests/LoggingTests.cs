using DejvidSmth.EfficientHttpClientBodyLogging;
using FluentAssertions;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp;
using System.Text;

namespace EfficientHttpClientBodyLogging.Tests;

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
    public void Logging_doesnt_break_when_sending_null_content()
    {

    }

    [Fact]
    public void Logging_doesnt_break_when_sending_empty_content()
    {

    }

    [Fact]
    public void Logging_doesnt_break_when_receiving_empty_content()
    {

    }

    [Fact]
    public void Request_logging_for_all_requests_can_be_disabled()
    {

    }

    [Fact]
    public void Response_logging_for_all_requests_can_be_disabled()
    {

    }

    //TODO: verify disabling certain content types (related to above tests)

    //TODO: verify that both sync and async send works
    //TODO: verify more content types, not just JSON
    //TODO: verify that headers are properly set

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

        var resp = await client.SendAsync(msg);
        resp.EnsureSuccessStatusCode();
    }
}