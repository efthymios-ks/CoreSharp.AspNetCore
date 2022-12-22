using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreSharp.AspNetCore.Swagger.OperationFilters.Common;

/// <inheritdoc cref="ParameterMapOperationFilter" />
public abstract class ParameterMapOperationFilterBase
{
    // Fields 
    private static readonly IDictionary<string, Action<HttpContext>> _pipelineHandlers
        = new Dictionary<string, Action<HttpContext>>();

    // Methods 
    internal static Task ProcessAsync(HttpContext httpContext)
    {
        foreach (var pipelineHandle in _pipelineHandlers.Values)
            pipelineHandle(httpContext);

        return Task.CompletedTask;
    }

    protected void RegisterCurrentPipelineHandler()
        => _pipelineHandlers[GetType().FullName] = OnPipelineProcess;

    protected abstract void OnPipelineProcess(HttpContext httpContext);
}
