using CoreSharp.Extensions;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CoreSharp.AspNetCore.Swagger.OperationFilters.Common;

public abstract partial class ParameterMapBase
{
    public static class PipelineFunctions
    {
        public static void HeaderToHeader(HttpContext httpContext, string fromHeader, string toHeader)
        {
            // Validate 
            if (string.IsNullOrWhiteSpace(fromHeader))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(toHeader))
            {
                return;
            }

            // Try get value 
            if (!TryRemoveHeader(httpContext.Request, fromHeader, out var value))
            {
                return;
            }

            // Map to new 
            TrySetHeader(httpContext.Request, toHeader, value);
        }

        public static void HeaderToQuery(HttpContext httpContext, string fromHeader, string toQuery)
        {
            // Validate 
            if (string.IsNullOrWhiteSpace(fromHeader))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(toQuery))
            {
                return;
            }

            // Try get value 
            if (!TryRemoveHeader(httpContext.Request, fromHeader, out var value))
            {
                return;
            }

            // Map to new 
            TrySetQueryParameter(httpContext.Request, toQuery, value);
        }

        public static void QueryToHeader(HttpContext httpContext, string fromQuery, string toHeader)
        {
            // Validate 
            if (string.IsNullOrWhiteSpace(fromQuery))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(toHeader))
            {
                return;
            }

            // Try get value 
            if (!TryRemoveQueryParameter(httpContext.Request, fromQuery, out var value))
            {
                return;
            }

            // Map to new 
            TrySetHeader(httpContext.Request, toHeader, value);
        }

        public static void QueryToQuery(HttpContext httpContext, string fromQuery, string toQuery)
        {
            // Validate 
            if (string.IsNullOrWhiteSpace(fromQuery))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(toQuery))
            {
                return;
            }

            var request = httpContext.Request;
            var queryParameters = QueryStringToDictionary(request.QueryString);
            if (!queryParameters.TryRemove(fromQuery, out var value))
            {
                return;
            }

            queryParameters[toQuery] = value;
            request.QueryString = DictionaryToQueryString(queryParameters);
        }

        private static bool TryRemoveHeader(HttpRequest httpRequest, string headerKey, out string value)
        {
            if (!httpRequest.Headers.TryGetValue(headerKey, out var values))
            {
                value = null;
                return false;
            }

            value = values.FirstOrDefault();
            httpRequest.Headers.Remove(headerKey);
            return true;
        }

        private static bool TrySetHeader(HttpRequest httpRequest, string headerKey, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            httpRequest.Headers[headerKey] = value;
            return true;
        }

        private static bool TryRemoveQueryParameter(HttpRequest httpRequest, string queryParameterKey, out string value)
        {
            var queryParameters = QueryStringToDictionary(httpRequest.QueryString);
            if (!queryParameters.TryRemove(queryParameterKey, out value))
            {
                value = null;
                return false;
            }

            httpRequest.QueryString = DictionaryToQueryString(queryParameters);
            return true;
        }

        private static bool TrySetQueryParameter(HttpRequest httpRequest, string queryParameterKey, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var queryParameters = QueryStringToDictionary(httpRequest.QueryString);
            queryParameters[queryParameterKey] = value;
            httpRequest.QueryString = DictionaryToQueryString(queryParameters);
            return true;
        }

        private static IDictionary<string, string> QueryStringToDictionary(QueryString queryString)
        {
            var queryParameterCollection = HttpUtility.ParseQueryString(queryString.Value);
            return queryParameterCollection.AllKeys
                  .ToDictionary(key => key, key => queryParameterCollection[key]);
        }

        private static QueryString DictionaryToQueryString(IDictionary<string, string> queryParameters)
            => QueryString.Create(queryParameters);
    }
}