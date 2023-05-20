using CoreSharp.AspNetCore.ActionFilters.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CoreSharp.AspNetCore.ActionFilters;

public sealed class DuplicateRequestGuardByBodyAttribute : DuplicateRequestGuardBaseAttribute
{
    // Properties
    /// <summary>
    /// Argument order of endpoint that <see cref="HttpResponse.Body"/> is mapped to.
    /// </summary>
    public int ArgumentOrder { get; set; }

    /// <summary>
    /// <see cref="HttpResponse.Body"/> property name to use as unique token.
    /// </summary>
    public string PropertyName { get; set; } = "Id";

    // Methods
    protected override Task<string> ExtractUniqueTokenAsync(ActionExecutingContext context)
    {
        using var jsonDocument = GetArgumentAsJsonDocument(context);
        var token = GetPropertyValue(jsonDocument);
        return Task.FromResult(token);
    }

    /// <summary>
    /// Convert <see cref="HttpRequest.Body"/> to <see cref="JsonDocument"/>.
    /// </summary>
    [SuppressMessage("Usage", "CA2201:Do not raise reserved exception types", Justification = "<Pending>")]
    private JsonDocument GetArgumentAsJsonDocument(ActionExecutingContext context)
    {
        var arguments = context.ActionArguments.Values;
        if (ArgumentOrder < 0 || ArgumentOrder >= arguments.Count)
        {
            var message = $"`{nameof(ArgumentOrder)}` ({ArgumentOrder}) does not match any `{context.ActionDescriptor.DisplayName}` argument.";
            throw new IndexOutOfRangeException(message);
        }

        var argument = arguments.ElementAt(ArgumentOrder);
        var argumentAsJson = JsonSerializer.Serialize(argument);
        return JsonDocument.Parse(argumentAsJson);
    }

    private string GetPropertyValue(JsonDocument jsonDocument)
    {
        var properties = jsonDocument.RootElement.EnumerateObject();
        var property = properties.FirstOrDefault(p => NamesMatch(p.Name, PropertyName));
        return property.Value.GetString();

        static bool NamesMatch(string left, string right)
            => string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
    }
}