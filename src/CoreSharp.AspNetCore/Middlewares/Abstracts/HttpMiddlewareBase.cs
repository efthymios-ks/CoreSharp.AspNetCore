using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace CoreSharp.AspNetCore.Middlewares.Abstracts
{
    /// <inheritdoc cref="IMiddleware" />
    public abstract class HttpMiddlewareBase
    {
        //Constructors
        protected HttpMiddlewareBase(RequestDelegate next)
            => Next = next ?? throw new ArgumentNullException(nameof(next));

        //Properties
        public RequestDelegate Next { get; }

        //Methods
        public abstract Task InvokeAsync(HttpContext context);
    }
}
