using CoreSharp.Models.Pages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CoreSharp.AspNetCore.Extensions;

/// <summary>
/// <see cref="ControllerBase"/> extensions.
/// </summary>
public static class ControllerBaseExtensions
{
    /// <inheritdoc cref="HttpContextExtensions.GetLinkedPage{TEntity}(HttpContext, Page{TEntity})"/>
    public static LinkedPage<TEntity> GetLinkedPage<TEntity>(this ControllerBase controller, Page<TEntity> page)
        => controller?.HttpContext.GetLinkedPage(page);
}
