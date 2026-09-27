using BankingApi.Attributes;
using System.Diagnostics;
using System.Text;

namespace BankingApi.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        public RequestResponseLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint?.Metadata.GetMetadata<SkipLoggingAttribute>() != null)
            {
                await _next(context);
                return;
            }
            context.Request.EnableBuffering();

            var requestBody = await ReadRequestBodyAsync(context.Request);

            var originalResponseBodyStream = context.Response.Body;
            using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            var stopwatch = Stopwatch.StartNew();

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();

                responseBodyStream.Seek(0, SeekOrigin.Begin);
                var responseBody = await new StreamReader(responseBodyStream).ReadToEndAsync();
                responseBodyStream.Seek(0, SeekOrigin.Begin);

                await responseBodyStream.CopyToAsync(originalResponseBodyStream);

                _logger.LogInformation(
                    "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms. RequestBody: {RequestBody} | ResponseBody: {ResponseBody}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    string.IsNullOrWhiteSpace(requestBody) ? "[Empty]" : requestBody,
                    string.IsNullOrWhiteSpace(responseBody) ? "[Empty]" : responseBody);
            }
        }

        private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
        {
            if (request.ContentLength == null || request.ContentLength == 0)
                return string.Empty;

            request.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(
                request.Body,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true);

            var body = await reader.ReadToEndAsync();
            request.Body.Seek(0, SeekOrigin.Begin); 

            return body;
        }
    }
}
