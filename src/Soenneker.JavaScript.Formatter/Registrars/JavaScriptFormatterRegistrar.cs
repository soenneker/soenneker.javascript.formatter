using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.JavaScript.Formatter.Abstract;
using Soenneker.Utils.Directory.Registrars;
using Soenneker.Utils.File.Registrars;

namespace Soenneker.JavaScript.Formatter.Registrars;

/// <summary>
/// A utility library that formats and normalizes JavaScript strings and files.
/// </summary>
public static class JavaScriptFormatterRegistrar
{
    /// <summary>
    /// Adds <see cref="IJavaScriptFormatter"/> as a singleton service. <para/>
    /// </summary>
    public static IServiceCollection AddJavaScriptFormatterAsSingleton(this IServiceCollection services)
    {
        services.AddFileUtilAsSingleton().AddDirectoryUtilAsSingleton().TryAddSingleton<IJavaScriptFormatter, JavaScriptFormatter>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="IJavaScriptFormatter"/> as a scoped service. <para/>
    /// </summary>
    public static IServiceCollection AddJavaScriptFormatterAsScoped(this IServiceCollection services)
    {
        services.AddFileUtilAsScoped().AddDirectoryUtilAsScoped().TryAddScoped<IJavaScriptFormatter, JavaScriptFormatter>();

        return services;
    }
}