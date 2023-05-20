using CoreSharp.AspNetCore.Extensions;
using CoreSharp.AspNetCore.Middlewares.Abstracts;
using CoreSharp.AspNetCore.Utilities;
using CoreSharp.Constants;
using CoreSharp.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace CoreSharp.AspNetCore.Middlewares;

public sealed class ErrorHandleMiddleware : HttpMiddlewareBase
{
    // Constructors
    public ErrorHandleMiddleware(RequestDelegate next)
        : base(next)
    {
    }

    // Methods
    public override async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await Next(httpContext);
        }
        catch (Exception exception) when (!httpContext.Response.HasStarted)
        {
            httpContext.SetExceptionHandlerFeature(exception);
            await HandleExceptionAsync(httpContext);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext httpContext)
    {
        _ = httpContext ?? throw new ArgumentNullException(nameof(httpContext));

        var response = httpContext.Response;
        var problemDetails = ProblemDetailsX.Create(httpContext);
        await WriteResponseAsync(response, problemDetails);
    }

    private static async Task WriteResponseAsync(HttpResponse httpResponse, ProblemDetails problemDetails)
    {
        _ = httpResponse ?? throw new ArgumentNullException(nameof(httpResponse));

        if (httpResponse.HasStarted)
        {
            return;
        }

        var jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        var problemDetailsAsJson = JsonSerializer.Serialize(problemDetails, jsonSerializerOptions);

        httpResponse.Clear();
        httpResponse.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;
        httpResponse.ContentType = MediaTypeNamesX.Application.ProblemJson;
        await httpResponse.WriteAsync(problemDetailsAsJson);
    }
}
