using CoreSharp.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace CoreSharp.Models.Middlewares
{
    public class RequestLogMiddleware
    {
        //Fields 
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly RequestDelegate _next;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly ILogger _logger;

        //Constructors
        public RequestLogMiddleware(RequestDelegate next, ILogger<RequestLogMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        //Methods 
        public async Task InvokeAsync(HttpContext context)
        {
            //Run pipeline
            var stopwatch = Stopwatch.StartNew();
            await _next(context);
            stopwatch.Stop();

            //Log entry 
            var duration = stopwatch.Elapsed;
            var message = GetRequestLogEntry(context, duration);
            _logger.LogInformation(message);
        }

        private static string GetRequestLogEntry(HttpContext context, TimeSpan duration)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            var request = context.Request;
            var response = context.Response;

            //Format values 
            var requestPath = $"{request.Host}{request.Path}";
            var status = $"{response.StatusCode} {(HttpStatusCode)response.StatusCode}";
            var responseSizeAsString = ((ulong?)response.ContentLength)?.ToComputerSizeCI();
            var durationAsString = duration.ToStringReadable();

            //Build and return 
            return $"[{DateTime.UtcNow:u}] {request.Method} > {requestPath} > {status} > {responseSizeAsString} in {durationAsString}";
        }
    }
}
