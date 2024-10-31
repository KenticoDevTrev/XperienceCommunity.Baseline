using Account.Models;
using CMS.Core;
using Core.Services;
using MVCCaching;

namespace Account.Repositories.Implementations
{
    /// <summary>
    /// TODO: Once WebsiteChannelSettings are configurable, should add these Urls to them.
    /// </summary>
    /// <param name="urlResolver"></param>
    /// <param name="settingsService"></param>
    /// <param name="cacheDependencyBuilderFactory"></param>
    /// <param name="passwordPolicySettings"></param>
    public class AccountSettingsRepository(IUrlResolver urlResolver, 
        ISettingsService settingsService,
        ICacheDependencyBuilderFactory cacheDependencyBuilderFactory,
        PasswordPolicySettings passwordPolicySettings) : IAccountSettingsRepository
    {
        private readonly IUrlResolver _urlResolver = urlResolver;
        private readonly ISettingsService _settingsService = settingsService;
        private readonly ICacheDependencyBuilderFactory _cacheDependencyBuilderFactory = cacheDependencyBuilderFactory;
        private readonly PasswordPolicySettings _passwordPolicySettings = passwordPolicySettings;

        public Task<string> GetAccountConfirmationUrlAsync(string fallBackUrl)
        {
            return Task.FromResult(fallBackUrl);
            /*
            _ = _cacheDependencyBuilderFactory.Create()
                .Object(SettingsKeyInfo.OBJECT_TYPE, "AccountConfirmationUrl");
            string url = _settingsService["AccountConfirmationUrl"];
            url = !string.IsNullOrWhiteSpace(url) ? url : fallBackUrl;
            return Task.FromResult(_urlResolver.ResolveUrl(url));
            */
        }

        public Task<string> GetAccountForgottenPasswordResetUrlAsync(string fallBackUrl)
        {
            return Task.FromResult(fallBackUrl);
            /*
            _ = _cacheDependencyBuilderFactory.Create()
                .Object(SettingsKeyInfo.OBJECT_TYPE, "AccountForgottenPasswordResetUrl");
            string url = _settingsService["AccountForgottenPasswordResetUrl"];
            url = !string.IsNullOrWhiteSpace(url) ? url : fallBackUrl;
            return Task.FromResult(_urlResolver.ResolveUrl(url));
            */
        }

        public Task<string> GetAccountForgotPasswordUrlAsync(string fallBackUrl)
        {
            return Task.FromResult(fallBackUrl);
            /*
            _ = _cacheDependencyBuilderFactory.Create()
                .Object(SettingsKeyInfo.OBJECT_TYPE, "AccountForgotPasswordUrl");
            string url = _settingsService["AccountForgotPasswordUrl"];
            url = !string.IsNullOrWhiteSpace(url) ? url : fallBackUrl;
            return Task.FromResult(_urlResolver.ResolveUrl(url));
            */
        }

        public Task<string> GetAccountLoginUrlAsync(string fallBackUrl)
        {
            return Task.FromResult(fallBackUrl);
            /*
            _ = _cacheDependencyBuilderFactory.Create()
                .Object(SettingsKeyInfo.OBJECT_TYPE, "AccountLoginUrl");
            string url = _settingsService["AccountLoginUrl"];
            url = !string.IsNullOrWhiteSpace(url) ? url : fallBackUrl;
            return Task.FromResult(_urlResolver.ResolveUrl(url));
            */
        }

        public Task<string> GetAccountRegistrationUrlAsync(string fallBackUrl)
        {
            return Task.FromResult(fallBackUrl);
            /*
            _ = _cacheDependencyBuilderFactory.Create()
                .Object(SettingsKeyInfo.OBJECT_TYPE, "AccountRegistrationUrl");
            string url = _settingsService["AccountRegistrationUrl"];
            url = !string.IsNullOrWhiteSpace(url) ? url : fallBackUrl;
            return Task.FromResult(_urlResolver.ResolveUrl(url));
            */
        }

        public Task<string> GetAccountMyAccountUrlAsync(string fallBackUrl)
        {
            return Task.FromResult(fallBackUrl);
            /*
            _ = _cacheDependencyBuilderFactory.Create()
                .Object(SettingsKeyInfo.OBJECT_TYPE, "AccountMyAccountUrl");
            string url = _settingsService["AccountMyAccountUrl"];
            url = !string.IsNullOrWhiteSpace(url) ? url : fallBackUrl;
            return Task.FromResult(_urlResolver.ResolveUrl(url));
            */
        }

        public Task<bool> GetAccountRedirectToAccountAfterLoginAsync()
        {
            return Task.FromResult(false);
            /*
            _ = _cacheDependencyBuilderFactory.Create()
                .Object(SettingsKeyInfo.OBJECT_TYPE, "AccountRedirectToAccountAfterLogin");
            return Task.FromResult(ValidationHelper.GetBoolean(_settingsService["AccountRedirectToAccountAfterLogin"], false));
            */
        }

        public Task<string> GetAccountLogOutUrlAsync(string fallBackUrl)
        {
            return Task.FromResult(fallBackUrl);
            /*
            _ = _cacheDependencyBuilderFactory.Create()
                .Object(SettingsKeyInfo.OBJECT_TYPE, "AccountLogOutUrl");
            string url = _settingsService["AccountLogOutUrl"];
            url = !string.IsNullOrWhiteSpace(url) ? url : fallBackUrl;
            return Task.FromResult(_urlResolver.ResolveUrl(url));
            */
        }

        public Task<string> GetAccountResetPasswordUrlAsync(string fallBackUrl)
        {
            return Task.FromResult(fallBackUrl);
            /*
            _ = _cacheDependencyBuilderFactory.Create()
                .Object(SettingsKeyInfo.OBJECT_TYPE, "AccountResetPassword");
            string url = _settingsService["AccountResetPassword"];
            url = !string.IsNullOrWhiteSpace(url) ? url : fallBackUrl;
            return Task.FromResult(_urlResolver.ResolveUrl(url));
            */
        }

        public Task<string> GetAccessDeniedUrlAsync(string fallBackUrl)
        {
            return Task.FromResult(fallBackUrl);
            /*
            _ = _cacheDependencyBuilderFactory.Create()
                .Object(SettingsKeyInfo.OBJECT_TYPE, "AccessDeniedUrl");
            string url = _settingsService["AccessDeniedUrl"];
            url = !string.IsNullOrWhiteSpace(url) ? url : fallBackUrl;
            return Task.FromResult(_urlResolver.ResolveUrl(url));
            */
        }

        public Task<PasswordPolicySettings> GetPasswordPolicyAsync()
        {
            return Task.FromResult(GetPasswordPolicy());
        }

        public PasswordPolicySettings GetPasswordPolicy() => _passwordPolicySettings;
    }
}
