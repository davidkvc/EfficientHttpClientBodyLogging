using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DavidKvc.Extensions.Http.BodyLogging.Tests;

static internal class HttpRequestMessageExtensions
{
    public static void ConsumeContent(this HttpRequestMessage req)
    {
        var contentStream = req.Content!.ReadAsStream();
        using var sr = new StreamReader(contentStream);
        sr.ReadToEnd();
    }

    public static async Task ConsumeContentAsync(this HttpRequestMessage req)
    {
        var contentStream = await req.Content!.ReadAsStreamAsync();
        using var sr = new StreamReader(contentStream);
        await sr.ReadToEndAsync();
    }
}
