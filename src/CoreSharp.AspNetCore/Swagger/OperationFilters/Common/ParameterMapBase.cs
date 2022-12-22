using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CoreSharp.AspNetCore.Swagger.OperationFilters.Common;

/// <summary>
/// Use to map user request parameters to Swagger lists and vice-versa.<br/>
/// 1. Create custom parameter filter(s).<br/>
/// You may use <see cref="PipelineFunctions"/> for convenience.
/// <code>
/// public sealed class CultureParameterMapOperationFilter : ParameterMapBase
/// {
///     // Properties  
///     protected override string ParameterName { get; } = &quot;Culture&quot;;
/// 
///     protected override ParameterLocation ParameterLocation { get; } = ParameterLocation.Header;
/// 
///     protected override IEnumerable&lt;string&gt; ParameterSource { get; }
///         = new[] { &quot;en&quot;, &quot;el&quot;, &quot;bg&quot; }
///             .Select(cultureName =&gt; CultureInfo.GetCultureInfo(cultureName).DisplayName);
/// 
///     // Methods 
///     protected override bool ShouldApply(OpenApiOperation operation, OperationFilterContext context)
///         => context.ApiDescription.HttpMethod == HttpMethods.Get;
///        
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
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public abstract partial class ParameterMapBase : IOperationFilter
{
    // Fields 
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IDictionary<string, Action<HttpRequest>> _pipelineCallbacks
        = new Dictionary<string, Action<HttpRequest>>();

    // Constructors 
    public ParameterMapBase()
        => AddMeToPipeline();

    // Properties
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
        => ToString();
    protected abstract string ParameterName { get; }
    protected abstract ParameterLocation ParameterLocation { get; }
    protected abstract IEnumerable<string> ParameterSource { get; }

    protected virtual bool AllowEmptyValues { get; }
    protected virtual string DefaultValueKey { get; }

    // Methods 
    public override string ToString()
        => $"Location: {ParameterLocation}, Name: {ParameterName}";

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (!ShouldApply(operation, context))
            return;

        operation.Parameters ??= new List<OpenApiParameter>();
        operation.Parameters.Add(CreateOpenApiParameter());
    }

    protected virtual bool ShouldApply(OpenApiOperation operation, OperationFilterContext context)
        => true;

    private OpenApiParameter CreateOpenApiParameter()
        => new()
        {
            Name = ParameterName,
            AllowEmptyValue = AllowEmptyValues,
            In = ParameterLocation,
            Example = GetDefaultValue(),
            Schema = new()
            {
                Enum = GetParameterEnums()
            }
        };

    private IOpenApiAny GetDefaultValue()
    {
        if (string.IsNullOrWhiteSpace(DefaultValueKey))
            return null;

        if (ParameterSource?.Contains(DefaultValueKey) is not true)
            return null;

        return new OpenApiString(DefaultValueKey);
    }

    private IList<IOpenApiAny> GetParameterEnums()
        => (ParameterSource ?? Enumerable.Empty<string>())
            .Select(key => new OpenApiString(key))
            .ToList<IOpenApiAny>();

    private void AddMeToPipeline()
        => _pipelineCallbacks[GetType().FullName] = OnPipelineProcess;

    internal static Task ProcessPipelineAsync(HttpRequest httpRequest)
    {
        foreach (var pipelineHandle in _pipelineCallbacks.Values)
            pipelineHandle(httpRequest);

        return Task.CompletedTask;
    }

    protected abstract void OnPipelineProcess(HttpRequest httpRequest);
}