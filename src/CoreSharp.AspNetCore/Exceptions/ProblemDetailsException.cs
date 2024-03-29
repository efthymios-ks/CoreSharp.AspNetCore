﻿using CoreSharp.AspNetCore.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;

namespace CoreSharp.AspNetCore.Exceptions;

public sealed class ProblemDetailsException : Exception
{
    // Constructors
    public ProblemDetailsException(HttpStatusCode httpStatusCode)
        : this(ProblemDetailsX.Create(httpStatusCode))
        => ProblemDetails = null;

    public ProblemDetailsException(HttpContext httpContext)
        : this(ProblemDetailsX.Create(httpContext))
    {
    }

    public ProblemDetailsException(string type, string title, HttpStatusCode status, string detail = null, string instance = null)
        : this(ProblemDetailsX.Create(type, title, status, detail, instance))
    {
    }

    public ProblemDetailsException(ProblemDetails problemDetails)
        : base($"{problemDetails?.Type} > {problemDetails?.Title}")
    {
        ArgumentNullException.ThrowIfNull(problemDetails);

        ProblemDetails = problemDetails;
    }

    // Properties
    public ProblemDetails ProblemDetails { get; }
}
