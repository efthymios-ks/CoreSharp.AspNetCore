using CoreSharp.AspNetCore.Swagger.OperationFilters.Common;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace CoreSharp.AspNetCore.Swagger.Middlewares;

/// <inheritdoc cref="ParameterMapBase" />
public sealed class ParameterMapMiddleware
{
    // Fields
    private readonly RequestDelegate _requestDelegate;

    // Constructors 
    public ParameterMapMiddleware(RequestDelegate requestDelegate)
        => _requestDelegate = requestDelegate;

    // Methods
    public async Task InvokeAsync(HttpContext httpContext)
    {
        await ParameterMapBase.ProcessPipelineAsync(httpContext.Request);
        await _requestDelegate(httpContext);
    }
}