
using DavidKvc.Extensions.Http.BodyLogging;
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

using var resp = await client.PostAsJsonAsync("https://httpbin.org/post", new
{
    data = "hello world"
});

resp.EnsureSuccessStatusCode();

var respData = await resp.Content.ReadFromJsonAsync<JsonElement>();


