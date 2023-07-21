using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;

namespace CoreSharp.AspNetCore.ActionFilters.Abstracts;

/// <summary>
/// Base class for guard implementation.<br/>
/// Use subclasses on a controller or on controller methods to protect from duplicate requests.<br/>
/// <code>
/// // Usage 
/// [HttpPost]
/// [HeaderDuplicateRequestGuard]
/// public async Task&lt;IActionResult> CreateAsync(User user)
/// </code>
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public abstract class DuplicateRequestGuardBaseAttribute : Attribute, IAsyncActionFilter, IAsyncResultFilter
{
    // Fields
    private const string OriginalTagHeaderKey = "Guard-Original-Tag";
    private static readonly ConcurrentDictionary<string, string> _activeRequests = new();

    // Methods
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // If idempotent method, ignore 
        var httpContext = context.HttpContext;
        var httpRequest = httpContext.Request;
        if (IsIdempotent(httpRequest))
        {
            return;
        }

        // Check if current unique token exists
        var currentToken = await ExtractUniqueTokenAsync(context);
        if (string.IsNullOrWhiteSpace(currentToken))
        {
            var message = $"`{nameof(DuplicateRequestGuardBaseAttribute)}.{nameof(ExtractUniqueTokenAsync)}()` returned empty value. Make sure it returns a unique value.";
            await WriteResponseAsync(httpContext, HttpStatusCode.UnprocessableEntity, message);
            return;
        }

        // Compare with previous (cached) unique token
        var previousToken = RetrieveUniqueToken(httpRequest);
        if (currentToken == previousToken)
        {
            var message = "Duplicate request disallowed.";
            await WriteResponseAsync(httpContext, HttpStatusCode.TooManyRequests, message);
            return;
        }

        // Store new token and tag request as original 
        StoreUniqueToken(httpRequest, currentToken);
        TagOriginalRequest(httpRequest);

        // Continue 
        await next();
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        // If an original request finishes, clear token 
        var httpContext = context.HttpContext;
        var httpRequest = httpContext.Request;
        if (IsOriginalRequest(httpRequest))
        {
            ClearUniqueToken(httpRequest);
        }

        await next();
    }

    /// <summary>
    /// Based on RFC2616-SEC9. 
    /// </summary>
    private static bool IsIdempotent(HttpRequest httpRequest)
    {
        var idempotentMethods = new[]
        {
            HttpMethods.Get, HttpMethods.Head,
            HttpMethods.Options, HttpMethods.Trace,
            HttpMethods.Put, HttpMethods.Patch,
            HttpMethods.Delete
        };
        var method = httpRequest.Method;
        return idempotentMethods.Contains(method);
    }

    /// <summary>
    /// Extract unique token from <see cref="ActionExecutingContext"/>.
    /// </summary>
    protected abstract Task<string> ExtractUniqueTokenAsync(ActionExecutingContext context);

    /// <summary>
    /// Generate unique hash for <see cref="HttpRequest"/>.
    /// It is used as key for storing unique token.
    /// </summary>
    private static string GenerateRequestHash(HttpRequest httpRequest)
        => $"{httpRequest.Method}+{httpRequest.Path}";

    /// <summary>
    /// Retrieve unique <see cref="HttpRequest"/> cached token.
    /// </summary>
    private static string RetrieveUniqueToken(HttpRequest httpRequest)
    {
        var requestHash = GenerateRequestHash(httpRequest);
        _activeRequests.TryGetValue(requestHash, out var value);
        return value;
    }

    /// <summary>
    /// Cache unique <see cref="HttpRequest"/> token.
    /// </summary>
    private static void StoreUniqueToken(HttpRequest httpRequest, string token)
    {
        var requestHash = GenerateRequestHash(httpRequest);
        _activeRequests.TryAdd(requestHash, token);
    }

    /// <summary>
    /// Clear unique <see cref="HttpRequest"/> cached token.
    /// </summary>
    private static void ClearUniqueToken(HttpRequest httpRequest)
    {
        var requestHash = GenerateRequestHash(httpRequest);
        _activeRequests.TryRemove(requestHash, out var _);
    }

    private static async Task WriteResponseAsync(HttpContext httpContext, HttpStatusCode httpStatusCode, string message)
    {
        var response = httpContext.Response;

        response.Clear();
        response.StatusCode = (int)httpStatusCode;
        response.ContentType = MediaTypeNames.Application.Json;
        await response.WriteAsync(message);
    }

    /// <summary>
    /// Tag <see cref="HttpRequest"/> as original.
    /// Used before action execution.
    /// </summary>
    private static void TagOriginalRequest(HttpRequest httpRequest)
        => httpRequest.Headers.Add(OriginalTagHeaderKey, new StringValues(string.Empty));

    /// <summary>
    /// Check if <see cref="HttpRequest"/> is tagged as duplicate.
    /// Used after action execution.
    /// </summary>
    private static bool IsOriginalRequest(HttpRequest httpRequest)
        => httpRequest.Headers.ContainsKey(OriginalTagHeaderKey);
}
