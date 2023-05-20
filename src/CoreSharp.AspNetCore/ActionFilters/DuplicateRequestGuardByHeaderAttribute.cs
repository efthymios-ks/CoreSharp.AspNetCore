using CoreSharp.AspNetCore.ActionFilters.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace CoreSharp.AspNetCore.ActionFilters;

public sealed class DuplicateRequestGuardByHeaderAttribute : DuplicateRequestGuardBaseAttribute
{
    // Properties
    /// <summary>
    /// Header name to get unique token from <see cref="HttpRequest.Headers"/>.
    /// </summary>
    public string HeaderName { get; set; } = "Guard-Unique-Token";

    // Methods 
    protected override Task<string> ExtractUniqueTokenAsync(ActionExecutingContext context)
    {
        var request = context.HttpContext.Request;
        var headers = request.Headers;
        string token = null;
        if (headers.TryGetValue(HeaderName, out var values))
        {
            token = values[0];
        }

        return Task.FromResult(token);
    }
}
