using Microsoft.AspNetCore.Builder;

namespace Core.Models
{
    /// <summary>
    /// Indicates the configuration for the Baseline Core for Persistant storage (needed for for Post-Redirect-Get [ExportModelState]/[ImportModelState]/[ModelStateTransfer]/IModelStateService usage)
    /// 
    /// You can use TempDataCookiePersistantStorageConfiguration, SessionPersistantStorageConfiguration, or NoPersistantStorageConfiguration if you are doing your own thing or are not going to use the Model State services
    /// </summary>
    public interface IPersistantStorageConfiguration
    {
    }

    public record TempDataCookiePersistantStorageConfiguration(string TempDataCookieName = "TEMPDATA", Action<CookieTempDataProviderOptions>? TempDataCookieConfigurations = null) : IPersistantStorageConfiguration;

    public record SessionPersistantStorageConfiguration(string SessionCookieName = "SessionId", Action<SessionOptions>? SessionOptionsConfigurations = null) : IPersistantStorageConfiguration;

    public record NoPersistantStorageConfiguration() : IPersistantStorageConfiguration;
}
