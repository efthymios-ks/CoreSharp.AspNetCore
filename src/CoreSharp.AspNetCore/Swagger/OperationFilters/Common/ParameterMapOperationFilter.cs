using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

namespace CoreSharp.AspNetCore.Swagger.OperationFilters.Common;

/// <summary>
/// Use to map user request parameters to Swagger lists and vice-versa.<br/>
/// 1. Create custom parameter filter(s).
/// <code>
/// public sealed class CultureParameterMapOperationFilter : ParameterMapOperationFilter
/// {
///     // Properties  
///     protected override string ParameterName { get; } = &quot;Culture&quot;;
/// 
///     protected override ParameterLocation ParameterLocation { get; } = ParameterLocation.Header;
/// 
///     protected override IEnumerable&lt;string&gt; Source { get; }
///         = new[] { &quot;en&quot;, &quot;el&quot;, &quot;bg&quot; }
///             .Select(cultureName =&gt; CultureInfo.GetCultureInfo(cultureName).DisplayName);
/// 
///     // Methods 
///     protected override void OnPipelineProcess(HttpContext httpContext)
///     {
///         var headers = httpContext.Request.Headers;
/// 
///         // Try get value from request 
///         if (!headers.TryGetValue(ParameterName, out var parameterValues))
///             return;
/// 
///         // Clear 
///         headers.Remove(ParameterName);
/// 
///         // Get single value 
///         var parameterValue = parameterValues.FirstOrDefault();
///         if (string.IsNullOrWhiteSpace(parameterValue))
///             return;
/// 
///         // Map 
///         headers.Add(&quot;X-Culture&quot;, parameterValue);
///     }
/// }
/// </code>
/// 2. Register custom filter(s). 
/// <code>
/// static void ConfigureServices(IServiceCollection services)
///     => services.AddSwaggerGen(config => config.OperationFilter&lt;CultureParameterMapOperationFilter>());
/// </code>
/// 3. Register middleware.
/// <code>
/// static void Configure(IApplicationBuilder app)
///     => app.UseMiddleware&lt;SwaggerParameterMapMiddleware>();
/// </code>
/// <code>
/// </code>
/// </summary>
public abstract class ParameterMapOperationFilter : ParameterMapOperationFilterBase, IOperationFilter
{
    // Constructors 
    public ParameterMapOperationFilter()
        => RegisterCurrentPipelineHandler();

    // Properties
    protected abstract string ParameterName { get; }
    protected abstract ParameterLocation ParameterLocation { get; }
    protected abstract IEnumerable<string> Source { get; }

    protected virtual bool AllowEmptyValues { get; }
    protected virtual string DefaultParameterKey { get; }

    // Methods 
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();
        operation.Parameters.Add(CreateOpenApiParameter());
    }

    private OpenApiParameter CreateOpenApiParameter()
        => new()
        {
            Name = ParameterName,
            AllowEmptyValue = AllowEmptyValues,
            In = ParameterLocation,
            Example = GetDefaultParameter(),
            Schema = new()
            {
                Enum = GetParameterEnums()
            }
        };

    private IOpenApiAny GetDefaultParameter()
    {
        if (string.IsNullOrWhiteSpace(DefaultParameterKey))
            return null;

        if (Source?.Contains(DefaultParameterKey) is not true)
            return null;

        return new OpenApiString(DefaultParameterKey);
    }

    private IList<IOpenApiAny> GetParameterEnums()
        => (Source ?? Enumerable.Empty<string>())
            .Select(key => new OpenApiString(key))
            .ToList<IOpenApiAny>();
}