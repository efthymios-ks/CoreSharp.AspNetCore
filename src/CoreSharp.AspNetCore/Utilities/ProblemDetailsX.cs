using CoreSharp.AspNetCore.Exceptions;
using CoreSharp.AspNetCore.Extensions;
using CoreSharp.Utilities;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Hosting;
using System;
using System.Net;
using System.Text.RegularExpressions;

namespace CoreSharp.AspNetCore.Utilities;

/// <summary>
/// <see cref="ProblemDetails"/> utilities.
/// </summary>
public static class ProblemDetailsX
{
    /// <inheritdoc cref="Create(HttpContext)"/>
    public static ProblemDetails Create(ControllerBase controller, Exception exception)
    {
        var httpContext = controller.HttpContext;
        httpContext.SetExceptionHandlerFeature(exception);
        return Create(httpContext);
    }

    /// <inheritdoc cref="Create(string, string, HttpStatusCode, string, string)"/>
    public static ProblemDetails Create(HttpContext httpContext)
    {
        _ = httpContext ?? throw new ArgumentNullException(nameof(httpContext));

        // Get exception
        var exception = httpContext.Features.Get<IExceptionHandlerFeature>()?.Error;
        _ = exception ?? throw new ArgumentException($"Provided {nameof(HttpContext)} does not feature any {nameof(Exception)}.");

        // If ProblemDetailsException
        if (exception is ProblemDetailsException pde)
            return pde.ProblemDetails;

        // Else extract information
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var isProduction = string.Equals(environment, Environments.Production, StringComparison.OrdinalIgnoreCase);

        var type = exception.GetType().Name;
        string title = null;
        var httpStatusCode = HttpStatusCodeX.FromException(exception);
        string detail;
        var instance = httpContext.Request.Path;
        if (isProduction)
        {
            detail = exception.Message;
        }
        else
        {
            title = exception.Message;
            detail = Regex.Replace(exception.StackTrace, @"\r\n?|\n", "\n");
        }

        return Create(type, title, httpStatusCode, detail, instance);
    }

    /// <inheritdoc cref="Create(string, string, HttpStatusCode, string, string)"/>
    public static ProblemDetails Create(HttpStatusCode httpStatusCode, string detail = null, string instance = null)
        => Create(null, null, httpStatusCode, detail, instance);

    /// <summary>
    /// Create new <see cref="ProblemDetails"/>
    /// with arguments validation for empty and null.
    /// </summary>
    public static ProblemDetails Create(string type, string title, HttpStatusCode httpStatusCode, string detail = null, string instance = null)
    {
        if (string.IsNullOrWhiteSpace(type))
            type = HttpStatusCodeX.GetReferenceUrl(httpStatusCode);
        if (string.IsNullOrWhiteSpace(title))
            title = ReasonPhrases.GetReasonPhrase((int)httpStatusCode);

        return new()
        {
            Type = type,
            Title = title,
            Status = (int)httpStatusCode,
            Detail = detail,
            Instance = instance
        };
    }
}
