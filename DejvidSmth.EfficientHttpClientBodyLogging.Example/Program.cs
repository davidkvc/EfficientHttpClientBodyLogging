
using DejvidSmth.EfficientHttpClientBodyLogging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

var services = new ServiceCollection();

services.AddLogging(b => b.AddJsonConsole());
services.AddHttpClient("example")
    .AddHttpMessageHandler<HttpBodyLoggingHandler>();
services.Configure<HttpClientBodyLoggingOptions>(opts =>
{
    opts.BodyContentTypeAllowlist.Add(new MediaTypeHeaderValue("multipart/form-data"));
});
services.AddTransient<HttpBodyLoggingHandler>();

using var sp = services.BuildServiceProvider();

var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient("example");

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
msg.Content = originalRequestContent;

//msg.Content = HttpContentWrapper.WrapRequestContentForLogging(originalRequestContent, loggingOptions, logger);
////var mpc = new MultipartFormDataContent();
////using var cv = File.OpenRead("C:\\Users\\david\\Desktop\\david_kovac_cv.pdf");
////using var cvContent = new StreamContent(cv);
////cvContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
////mpc.Add(cvContent, "file", "file");
////msg.Content = WrapRequestContentForLogging(mpc, loggingOptions, logger);

using var resp = await client.SendAsync(msg);

resp.EnsureSuccessStatusCode();

var respData = await resp.Content.ReadFromJsonAsync<JsonElement>();


