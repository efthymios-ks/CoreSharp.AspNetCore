using Microsoft.Extensions.DependencyInjection;
using Server.Services;
using Server.Services.Interfaces;
using System;
using System.IO;
using System.Reflection;

namespace Server.Extensions;

/// <summary>
/// <see cref="IServiceCollection"/> extensions.
/// </summary>
internal static class IServiceCollectionExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        services.AddScoped<IDummyRepository, DummyRepository>();

        return services;
    }

    public static IServiceCollection AddAppSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(config =>
        {
            // Add xml documentation 
            var xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlFilePath = Path.Combine(AppContext.BaseDirectory, xmlFileName);
            config.IncludeXmlComments(xmlFilePath);
        });

        return services;
    }
}
