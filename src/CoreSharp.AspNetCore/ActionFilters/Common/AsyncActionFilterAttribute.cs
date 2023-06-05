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

        if (!await OnActionExecutingAsync(context))
        {
            return;
        }

        var resultContext = await next();
        await OnActionExecutedAsync(context, resultContext);
    }

    /// <summary>
    /// Called asynchronously when entering <see cref="OnActionExecutionAsync"/>.
    /// If false, will not continue down the action pipeline.
    /// </summary>
    protected virtual Task<bool> OnActionExecutingAsync(ActionExecutingContext context)
        => Task.FromResult(true);

    /// <summary>
    /// Called asynchronously when exiting <see cref="OnActionExecutionAsync"/>.
    /// </summary>
    protected virtual Task OnActionExecutedAsync(ActionExecutingContext context, ActionExecutedContext resultContext)
        => Task.CompletedTask;

    /// <inheritdoc />
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        if (!await OnResultExecutingAsync(context))
        {
            return;
        }

        var resultContext = await next();
        await OnResultExecutedAsync(context, resultContext);
    }

    /// <summary>
    /// Called asynchronously when entering <see cref="OnResultExecutingAsync"/>.
    /// If false, will not continue down the action pipeline.
    /// </summary>
    protected virtual Task<bool> OnResultExecutingAsync(ResultExecutingContext context)
        => Task.FromResult(true);

    /// <summary>
    /// Called asynchronously when exiting <see cref="OnResultExecutingAsync"/>.
    /// </summary>
    protected virtual Task OnResultExecutedAsync(ResultExecutingContext context, ResultExecutedContext resultContext)
        => Task.CompletedTask;
}
