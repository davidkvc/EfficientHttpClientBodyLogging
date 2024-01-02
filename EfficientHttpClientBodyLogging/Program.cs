
using ConsoleApp1;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

var loggingOptions = new HttpLoggingOptions();
loggingOptions.BodyContentTypeWhitelist.Add(new MediaTypeHeaderValue("multipart/form-data"));

using var lf = LoggerFactory.Create(b => b.AddConsole());

var logger = lf.CreateLogger("main");

using var client = new HttpClient();

using var msg = new HttpRequestMessage(HttpMethod.Post, "https://httpbin.org/post");

var data = new string[8];
for (int i = 0; i < data.Length; i++)
{
    data[i] = new string('a', 4000);
}
//msg.Content = WrapRequestContentForLogging(JsonContent.Create(new { 
//    name = "test",
//    count = 66,
//    data = data,
//}), loggingOptions, logger);
var mpc = new MultipartFormDataContent();
using var cv = File.OpenRead("C:\\Users\\david\\Desktop\\david_kovac_cv.pdf");
using var cvContent = new StreamContent(cv);
cvContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
mpc.Add(cvContent, "file", "file");
msg.Content = WrapRequestContentForLogging(mpc, loggingOptions, logger);

using var resp = await client.SendAsync(msg);

resp.EnsureSuccessStatusCode();

var responseStream = await PrepareResponseReadStream(resp.Content, loggingOptions, logger);
var respData = await JsonSerializer.DeserializeAsync<JsonElement>(responseStream);

static HttpContent WrapRequestContentForLogging(HttpContent content, HttpLoggingOptions loggingOptions, ILogger logger)
{
    var reqContentType = content.Headers.ContentType;

    if (reqContentType == null)
    {
        return content;
    }

    if (!MediaTypeHeaderValue.TryParse(reqContentType.ToString(), out var mediaType))
    {
        return content;
    }

    foreach (var supportedContentType in loggingOptions.BodyContentTypeWhitelist)
    {
        if (supportedContentType.MatchesMediaType(mediaType.MediaType))
        {
            var encoding = mediaType.Encoding ?? supportedContentType.Encoding ?? Encoding.UTF8;
            return new LoggingContent(content, encoding, loggingOptions.RequestBodyLogLimit, logger);
        }
    }

    return content;
}

static async Task<Stream> PrepareResponseReadStream(HttpContent content, HttpLoggingOptions loggingOptions, ILogger logger)
{
    var responseStream = await content.ReadAsStreamAsync();

    var respContentType = content.Headers.ContentType;

    if(respContentType == null)
    {
        return responseStream;
    }

    if (!MediaTypeHeaderValue.TryParse(respContentType.ToString(), out var mediaType))
    {
        return responseStream;
    }

    foreach (var supportedContentType in loggingOptions.BodyContentTypeWhitelist)
    {
        if (supportedContentType.MatchesMediaType(mediaType.MediaType))
        {
            var encoding = mediaType.Encoding ?? supportedContentType.Encoding ?? Encoding.UTF8;
            var responseLoggingStream = new LoggingStream(responseStream, encoding, loggingOptions.ResponseBodyLogLimit, LoggingStream.Content.ResponseBody, logger);
            return responseLoggingStream;
        }
    }

    return responseStream;
}
