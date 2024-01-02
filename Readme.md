# Efficient Request/Response body logging with C#'s HttpClient

This is an example of **memory-efficient** implementation of req/res body logging for C#'s HttpClient.

* Configure max size of req/resp body that will be read as a string and logged
* Use constant-sized linked list of pooled array segments to store bytes while req/resp is being processed

This is heavily inspired by existing implementation of req/res body logging for ASP.Net Core
so credits to Microsoft for that.

## Implementation details

Compared to other examples on the internet this implementation doesn't try to read request or response
body outside of standard req/res processing pipeline. If we wanted to first read the body from arbitrary
`HttpContent` we would have to read the entire content into memory which is inefficient, especially if
we only need part of the content.

Instead this implementation provides the `LoggingStream`, and we stream req/res body through that. As
the request is processed `LoggingStream` captures slice of the processed bytes up to configured limit
and eventually converts that to a string efficiently. Only then we log the captured string.