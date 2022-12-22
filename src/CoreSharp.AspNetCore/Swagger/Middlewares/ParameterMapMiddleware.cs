using CoreSharp.AspNetCore.Swagger.OperationFilters.Common;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace CoreSharp.AspNetCore.Swagger.Middlewares;

/// <inheritdoc cref="ParameterMapOperationFilter" />
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
        await ParameterMapOperationFilterBase.ProcessAsync(httpContext);
        await _requestDelegate(httpContext);
    }
}