using CoreSharp.AspNetCore.Extensions;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreSharp.AspNetCore.Extensions;

/// <summary>
/// <see cref="IServiceCollection"/> extensions.
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <inheritdoc cref="AddCors(IServiceCollection, string, IConfiguration)"/>
    public static IServiceCollection AddCors(this IServiceCollection services, IConfiguration configuration)
        => services.AddCors(null, configuration);

    /// <inheritdoc cref="AddCors(IServiceCollection, string, IConfigurationSection)"/>
    public static IServiceCollection AddCors(this IServiceCollection services, string policyKey, IConfiguration configuration)
        => services.AddCors(policyKey, configuration?.GetSection("Cors"));

    /// <inheritdoc cref="AddCors(IServiceCollection, string, IConfigurationSection)"/>
    public static IServiceCollection AddCors(this IServiceCollection services, IConfigurationSection configurationSection)
        => services.AddCors(null, configurationSection);

    /// <summary>
    /// <para>Configure CORS policy based on provided configuration.</para>
    /// <para>If <see href="PolicyKey"/> is null or empty, the policy configured is also set as default.</para>
    /// <code>
    /// // Sample `appsettings.json`
    /// {
    ///   "Cors": {
    ///     // Allow all
    ///     "AllowedOrigins": [ "*" ],
    ///
    ///     // No further configuration
    ///     "AllowedMethods": []
    ///
    ///     // Specific values
    ///     "AllowedHeaders": [ "get", "post", "put", "patch", "delete", "head", "trace", "options" ],
    ///
    ///     // Optional, can be completely removed
    ///     "ExposedHeaders": [ "Content-Disposition" ]
    ///   }
    /// }
    /// </code>
    /// </summary>
    public static IServiceCollection AddCors(this IServiceCollection services, string policyKey, IConfigurationSection configurationSection)
    {
        _ = services ?? throw new ArgumentNullException(nameof(services));
        _ = configurationSection ?? throw new ArgumentNullException(nameof(configurationSection));

        if (!configurationSection.Exists())
        {
            throw new KeyNotFoundException("No `Cors` configuration section found.");
        }

        IEnumerable<string> GetConfigurationValue(string key)
            => configurationSection.GetSection(key)
                                   .Get<IEnumerable<string>>()
                                   ?.Where(v => !string.IsNullOrWhiteSpace(v))
                                   .Select(v => v.Trim())
                                   ?? Enumerable.Empty<string>();

        static bool AllowAny(IEnumerable<string> values)
            => values?.Any(v => v == "*") is true;

        void ConfigureOrigins(CorsPolicyBuilder policy)
        {
            var origins = GetConfigurationValue("AllowedOrigins")
                            .Select(o => o.TrimEnd('/'))
                            .ToArray();

            if (origins.Length > 0)
            {
                if (AllowAny(origins))
                {
                    policy.AllowAnyOrigin();
                }
                else
                {
                    policy.WithOrigins(origins);
                }
            }
        }

        void ConfigureMethods(CorsPolicyBuilder policy)
        {
            var methods = GetConfigurationValue("AllowedMethods")
                            .Select(m => m.ToUpperInvariant())
                            .ToArray();

            if (methods.Length == 0)
            {
                return;
            }
            else if (AllowAny(methods))
            {
                policy.AllowAnyMethod();
                return;
            }

            // Validate arguments 
            static bool IsValidHttpMethod(string method)
            {
                var validMethods = new Func<string, bool>[]
                {
                    HttpMethods.IsGet,
                    HttpMethods.IsPost,
                    HttpMethods.IsPut,
                    HttpMethods.IsPatch,
                    HttpMethods.IsDelete,
                    HttpMethods.IsHead,
                    HttpMethods.IsTrace,
                    HttpMethods.IsOptions
                };

                return Array.Exists(validMethods, m => m(method));
            }

            if (Array.Find(methods, m => !IsValidHttpMethod(m)) is string invalidMethod)
            {
                throw new ArgumentOutOfRangeException($"Invalid entry at `Cors:AllowedMethods` ({invalidMethod}).");
            }

            policy.WithMethods(methods);
        }

        void ConfigureHeaders(CorsPolicyBuilder policy)
        {
            var headers = GetConfigurationValue("AllowedHeaders")
                            .ToArray();

            if (headers.Length > 0)
            {
                if (AllowAny(headers))
                {
                    policy.AllowAnyHeader();
                }
                else
                {
                    policy.WithHeaders(headers);
                }
            }
        }

        void ConfigureExposedHeaders(CorsPolicyBuilder policy)
        {
            var headers = GetConfigurationValue("ExposedHeaders")
                            .ToArray();

            if (headers.Length > 0)
            {
                policy.WithExposedHeaders(headers);
            }
        }

        void ConfigureCorsPolicy(CorsPolicyBuilder policy)
        {
            ConfigureOrigins(policy);
            ConfigureMethods(policy);
            ConfigureHeaders(policy);
            ConfigureExposedHeaders(policy);
        }

        void ConfigureCorsOptions(CorsOptions options)
        {
            if (string.IsNullOrWhiteSpace(policyKey))
            {
                options.AddDefaultPolicy(ConfigureCorsPolicy);
            }
            else
            {
                options.AddPolicy(policyKey, ConfigureCorsPolicy);
            }
        }

        services.AddCors(ConfigureCorsOptions);

        return services;
    }
}
