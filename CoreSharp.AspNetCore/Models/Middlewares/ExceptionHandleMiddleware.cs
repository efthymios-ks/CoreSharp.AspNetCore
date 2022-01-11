using CoreSharp.Extensions;
using CoreSharp.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace CoreSharp.Models.Middlewares
{
    public class ExceptionHandleMiddleware
    {
        //Fields 
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly RequestDelegate _next;

        //Constructors
        public ExceptionHandleMiddleware(RequestDelegate next)
            => _next = next;

        //Methods
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
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
                return;

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
}
