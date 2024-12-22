using Search.Installers;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionBaselineSearchRCLExtensions
    {
        /// <summary>
        /// Adds the Baseline Search RCL (Mainly the Page Type for the Page Template)
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddBaselineSearchRCL(this IServiceCollection services, Action<BaselineSearchInstallerOptions>? options)
        {
            var installerOptions = new BaselineSearchInstallerOptions();
            options?.Invoke(installerOptions);
            services.AddSingleton(installerOptions)
                .AddSingleton<BaselineSearchModuleInstaller>();
            
            return services;
        }
    }
}
