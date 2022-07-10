using CoreSharp.AspNetCore.Middlewares.Abstracts;
using CoreSharp.Extensions;
using CoreSharp.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace CoreSharp.AspNetCore.Middlewares;

public class RequestLogMiddleware : HttpMiddlewareBase
{
    //Fields 
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly ILogger _logger;

    //Constructors
    public RequestLogMiddleware(RequestDelegate next, ILogger<RequestLogMiddleware> logger)
        : base(next)
        => _logger = logger;

    //Methods 
    public override async Task InvokeAsync(HttpContext httpContext)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await Next(httpContext);
        }
        catch (Exception exception)
        {
            httpContext.Response.StatusCode = (int)HttpStatusCodeX.FromException(exception);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            var duration = stopwatch.Elapsed;
            var message = GetRequestLogEntry(httpContext, duration);
            _logger.LogInformation(message);
        }
    }

    private static string GetRequestLogEntry(HttpContext context, TimeSpan duration)
    {
        _ = context ?? throw new ArgumentNullException(nameof(context));

        var request = context.Request;
        var response = context.Response;

        //Format values 
        var requestPath = $"{request.Host}{request.Path}";
        var statusAsString = $"{response.StatusCode} {(HttpStatusCode)response.StatusCode}";
        var responseSizeAsString = ((ulong?)response.ContentLength)?.ToComputerSizeCI();
        var durationAsString = duration.ToStringReadable();

        //Build and return 
        return $"[{DateTime.UtcNow:u}] {request.Method} > {requestPath} > {statusAsString} > {responseSizeAsString} in {durationAsString}";
    }
}
