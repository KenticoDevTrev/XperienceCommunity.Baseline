using Admin.Installer;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceProviderStartingSiteExtensions
    {
        /// <summary>
        /// Adds the Starting Site's basic elements (Home Page, Basic Page, and optional Media Content Types
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IServiceCollection AddStartingSitePageTypes(this IServiceCollection services, Action<StartingSiteInstallationOptions>? options = null)
        {
            var installerOptions = new StartingSiteInstallationOptions();
            options?.Invoke(installerOptions);
            services.AddSingleton(installerOptions);
            services.AddSingleton<StartingSiteInstallerChannels>()
            services.AddSingleton<StartingSiteInstaller>();
            return services;
        }
    }
}
