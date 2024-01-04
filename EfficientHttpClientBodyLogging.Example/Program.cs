
using EfficientHttpClientBodyLogging;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

var loggingOptions = new HttpLoggingOptions();
loggingOptions.BodyContentTypeAllowlist.Add(new MediaTypeHeaderValue("multipart/form-data"));

using var lf = LoggerFactory.Create(b => b.AddConsole());

var logger = lf.CreateLogger("main");

using var client = new HttpClient();

using var msg = new HttpRequestMessage(HttpMethod.Post, "https://httpbin.org/post");

var data = new string[8];
for (int i = 0; i < data.Length; i++)
{
    data[i] = new string('a', 4000);
}

var originalRequestContent = JsonContent.Create(new
{
    name = "test",
    count = 66,
    data = data,
});

msg.Content = HttpContentWrapper.WrapRequestContentForLogging(originalRequestContent, loggingOptions, logger);
//var mpc = new MultipartFormDataContent();
//using var cv = File.OpenRead("C:\\Users\\david\\Desktop\\david_kovac_cv.pdf");
//using var cvContent = new StreamContent(cv);
//cvContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
//mpc.Add(cvContent, "file", "file");
//msg.Content = WrapRequestContentForLogging(mpc, loggingOptions, logger);

using var resp = await client.SendAsync(msg);

resp.EnsureSuccessStatusCode();

var responseStream = await HttpContentWrapper.PrepareResponseReadStream(resp.Content, loggingOptions, logger);
var respData = await JsonSerializer.DeserializeAsync<JsonElement>(responseStream);


