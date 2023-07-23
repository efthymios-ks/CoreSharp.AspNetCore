using CoreSharp.Extensions;
using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CoreSharp.AspNetCore.Middlewares.Abstracts;

public abstract class RequestLogMiddlewareBase : HttpMiddlewareBase
{
    // Constructors
    public RequestLogMiddlewareBase(RequestDelegate next)
        : base(next)
    {
    }

    // Methods 
    protected abstract void Log(string logEntry);

    public override async Task InvokeAsync(HttpContext httpContext)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await Next(httpContext);
        }
        finally
        {
            stopwatch.Stop();
            var duration = stopwatch.Elapsed;
            var message = GetRequestLogEntry(httpContext, duration);
            Log(message);
        }
    }

    private static string GetRequestLogEntry(HttpContext context, TimeSpan duration)
    {
        var request = context.Request;
        var response = context.Response;

        // Format values 
        var requestPath = $"{request.Host}{request.Path}";
        var responseStatusAsString = $"{response.StatusCode} {(HttpStatusCode)response.StatusCode}";
        var responseSizeAsString = ((ulong?)response.ContentLength)?.ToComputerSizeCI();
        var durationAsString = duration.ToStringReadable();

        var logEntry = new
        {
            DateUtc = DateTime.UtcNow.ToString("u"),
            RequestMethod = request.Method,
            RequestPath = requestPath,
            ResponseStatus = responseStatusAsString,
            responseSize = responseSizeAsString,
            Duration = durationAsString
        };

        return JsonSerialize(logEntry);
    }

    private static string JsonSerialize(object value)
    {
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        return JsonSerializer.Serialize(value, jsonSerializerOptions);
    }
}
