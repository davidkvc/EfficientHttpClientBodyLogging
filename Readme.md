# Efficient Request/Response body logging with C#'s HttpClient

This is an example of **memory-efficient** implementation of req/res body logging for C#'s HttpClient.

* Configure max size of req/resp body that will be read as a string and logged
* Use constant-sized linked list of pooled array segments to store bytes while req/resp is being processed

This is heavily inspired by existing implementation of req/res body logging for ASP.Net Core
so credits to Microsoft for that.