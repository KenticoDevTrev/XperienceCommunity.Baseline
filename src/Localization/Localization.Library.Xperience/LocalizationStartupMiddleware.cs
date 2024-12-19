using Localization.Library.Xperience.Repositories.Implementations;
using Localization.Repositories;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class LocalizationStartupMiddleware
    {
        /// <summary>
        /// Adds the Baseline Localization logic (was previously "UseLocalization").  Note that unlike the KX13 version (UseLocalization), this does not set up the Localization with controllers and view, that is now done separate.
        /// See https://github.com/KenticoDevTrev/XperienceCommunity.Baseline/blob/XbyK/starting-site/xbyk/MVC/MVC/Configuration/StartupConfigs.cs -> AddLocalizationAndControllerViews
        /// 
        /// Since Categories are now localizable objects in Xperience by Kentico, this can be added without localizing anything if you wish, just using basic Xperience functionality
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection UseBaselineLocalization(this IServiceCollection services) => services.AddScoped<ILocalizedCategoryCachedRepository, LocalizedCategoryCachedRepository>();
    }
}
