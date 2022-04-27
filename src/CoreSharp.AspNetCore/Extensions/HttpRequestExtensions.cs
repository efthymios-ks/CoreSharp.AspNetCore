using CoreSharp.Models.Pages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using static System.FormattableString;

namespace CoreSharp.AspNetCore.Extensions
{
    /// <summary>
    /// <see cref="HttpRequest"/> extensions.
    /// </summary>
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// Build new <see cref="LinkedPage{TEntity}"/>
        /// based on provided data.
        /// </summary>
        public static LinkedPage<TEntity> GetLinkedPage<TEntity>(this HttpRequest httpRequest, Page<TEntity> page)
        {
            _ = httpRequest ?? throw new ArgumentNullException(nameof(httpRequest));
            _ = page ?? throw new ArgumentNullException(nameof(page));

            if (httpRequest.Method != HttpMethods.Get)
                throw new InvalidOperationException($"Pagination only works on {nameof(HttpMethods.Get)} methods (provided {nameof(httpRequest.Method)}).");

            string BuildParameter(int pageNumber)
            {
                var queryBuilder = new QueryBuilder
                {
                    { nameof(PageOptions.PageNumber), Invariant($"{pageNumber}") },
                    { nameof(PageOptions.PageSize), Invariant($"{page.PageSize}") }
                };
                return $"{httpRequest.Path}{queryBuilder}";
            }

            var previousPage = page.HasPrevious ? BuildParameter(page.PageNumber - 1) : null;
            var nextPage = page.HasNext ? BuildParameter(page.PageNumber + 1) : null;

            return new(page, previousPage, nextPage);
        }
    }
}
