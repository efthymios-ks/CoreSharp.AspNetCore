using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Threading.Tasks;

namespace CoreSharp.AspNetCore.ActionFilters.Common;

/// <summary>
/// An abstract filter that asynchronously surrounds execution of the action and the action result. 
/// Subclasses should override <see cref="OnActionExecutingAsync"/> or <see cref="OnActionExecutedAsync"/>,
/// but not <see cref="OnActionExecutionAsync"/>. 
/// Similarly subclasses should override <see cref="OnResultExecutingAsync"/> or <see cref="OnResultExecutedAsync"/>,
/// but not <see cref="OnResultExecutionAsync"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public abstract class AsyncActionFilterAttribute :
    Attribute, IOrderedFilter, IAsyncActionFilter, IAsyncResultFilter
{
    // Properties
    /// <inheritdoc />
    public int Order { get; set; }

    // Methods 
    /// <inheritdoc />
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        await OnActionExecutingAsync(context);
        var resultContext = await next();
        await OnActionExecutedAsync(context, resultContext);
    }

    protected virtual Task OnActionExecutingAsync(ActionExecutingContext context)
        => Task.CompletedTask;

    protected virtual Task OnActionExecutedAsync(ActionExecutingContext context, ActionExecutedContext resultContext)
        => Task.CompletedTask;

    /// <inheritdoc />
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        await OnResultExecutingAsync(context);
        var resultContext = await next();
        await OnResultExecutedAsync(context, resultContext);
    }

    protected virtual Task OnResultExecutingAsync(ResultExecutingContext context)
        => Task.CompletedTask;

    protected virtual Task OnResultExecutedAsync(ResultExecutingContext context, ResultExecutedContext resultContext)
        => Task.CompletedTask;
}
