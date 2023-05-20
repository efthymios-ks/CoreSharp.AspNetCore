using CoreSharp.Models.Pages;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System;

namespace CoreSharp.AspNetCore.Extensions;

/// <summary>
/// <see cref="HttpContext"/> extensions.
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// Set <see cref="IExceptionHandlerFeature"/>
    /// and <see cref="IExceptionHandlerPathFeature"/>
    /// to given <see cref="Exception"/>.
    /// </summary>
    public static void SetExceptionHandlerFeature(this HttpContext httpContext, Exception exception)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var feature = new ExceptionHandlerFeature
        {
            Path = httpContext.Request.Path,
            Error = exception
        };
        httpContext.Features.Set<IExceptionHandlerFeature>(feature);
        httpContext.Features.Set<IExceptionHandlerPathFeature>(feature);
    }

    /// <inheritdoc cref="HttpRequestExtensions.GetLinkedPage{TEntity}(HttpRequest, Page{TEntity})"/>
    public static LinkedPage<TEntity> GetLinkedPage<TEntity>(this HttpContext httpContext, Page<TEntity> page)
        => httpContext?.Request.GetLinkedPage(page);
}
