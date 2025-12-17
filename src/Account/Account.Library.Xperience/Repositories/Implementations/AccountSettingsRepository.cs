using Account.Models;
using Core.Services;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MVCCaching;
using XperienceCommunity.ChannelSettings.Repositories;

namespace Account.Repositories.Implementations
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="urlResolver"></param>
    /// <param name="settingsService"></param>
    /// <param name="cacheDependencyBuilderFactory"></param>
    /// <param name="passwordPolicySettings"></param>
    public class AccountSettingsRepository(IUrlResolver urlResolver,
        ICacheDependencyScopedBuilderFactory cacheDependencyBuilderFactory,
        IChannelCustomSettingsRepository channelCustomSettingsRepository,
        IOptions<IdentityOptions> identityOptions) : IAccountSettingsRepository
    {
        private readonly IUrlResolver _urlResolver = urlResolver;
        private readonly ICacheDependencyScopedBuilderFactory _cacheDependencyBuilderFactory = cacheDependencyBuilderFactory;
        private readonly IChannelCustomSettingsRepository _channelCustomSettingsRepository = channelCustomSettingsRepository;
        private readonly IOptions<IdentityOptions> _identityOptions = identityOptions;

        public async Task<string> GetAccountConfirmationUrlAsync(string fallBackUrl)
        {
            _cacheDependencyBuilderFactory.Create().AddKeys(_channelCustomSettingsRepository.GetSettingModelDependencyKeys<AccountChannelSettings>());
            return _urlResolver.ResolveUrl((await _channelCustomSettingsRepository.GetSettingsModel<AccountChannelSettings>()).AccountConfirmationUrl.AsNullOrWhitespaceMaybe().GetValueOrDefault(fallBackUrl));
        }

        public async Task<string> GetAccountForgottenPasswordResetUrlAsync(string fallBackUrl)
        {
            _cacheDependencyBuilderFactory.Create().AddKeys(_channelCustomSettingsRepository.GetSettingModelDependencyKeys<AccountChannelSettings>());
            return _urlResolver.ResolveUrl((await _channelCustomSettingsRepository.GetSettingsModel<AccountChannelSettings>()).AccountForgottenPasswordResetUrl.AsNullOrWhitespaceMaybe().GetValueOrDefault(fallBackUrl));
        }

        public async Task<string> GetAccountForgotPasswordUrlAsync(string fallBackUrl)
        {
            _cacheDependencyBuilderFactory.Create().AddKeys(_channelCustomSettingsRepository.GetSettingModelDependencyKeys<AccountChannelSettings>());
            return _urlResolver.ResolveUrl((await _channelCustomSettingsRepository.GetSettingsModel<AccountChannelSettings>()).AccountForgotPasswordUrl.AsNullOrWhitespaceMaybe().GetValueOrDefault(fallBackUrl));
        }

        public async Task<string> GetAccountLoginUrlAsync(string fallBackUrl)
        {
            _cacheDependencyBuilderFactory.Create().AddKeys(_channelCustomSettingsRepository.GetSettingModelDependencyKeys<AccountChannelSettings>());
            return _urlResolver.ResolveUrl((await _channelCustomSettingsRepository.GetSettingsModel<AccountChannelSettings>()).AccountLoginUrl.AsNullOrWhitespaceMaybe().GetValueOrDefault(fallBackUrl));
        }

        public async Task<string> GetAccountTwoFormAuthenticationUrlAsync(string fallBackUrl)
        {
            _cacheDependencyBuilderFactory.Create().AddKeys(_channelCustomSettingsRepository.GetSettingModelDependencyKeys<AccountChannelSettings>());
            return _urlResolver.ResolveUrl((await _channelCustomSettingsRepository.GetSettingsModel<AccountChannelSettings>()).AccountTwoFormAuthenticationUrl.AsNullOrWhitespaceMaybe().GetValueOrDefault(fallBackUrl));
        }

        public async Task<string> GetAccountRegistrationUrlAsync(string fallBackUrl)
        {
            _cacheDependencyBuilderFactory.Create().AddKeys(_channelCustomSettingsRepository.GetSettingModelDependencyKeys<AccountChannelSettings>());
            return _urlResolver.ResolveUrl((await _channelCustomSettingsRepository.GetSettingsModel<AccountChannelSettings>()).AccountRegistrationUrl.AsNullOrWhitespaceMaybe().GetValueOrDefault(fallBackUrl));
        }

        public async Task<string> GetAccountMyAccountUrlAsync(string fallBackUrl)
        {
            _cacheDependencyBuilderFactory.Create().AddKeys(_channelCustomSettingsRepository.GetSettingModelDependencyKeys<AccountChannelSettings>());
            return _urlResolver.ResolveUrl((await _channelCustomSettingsRepository.GetSettingsModel<AccountChannelSettings>()).AccountMyAccountUrl.AsNullOrWhitespaceMaybe().GetValueOrDefault(fallBackUrl));
        }

        public async Task<bool> GetAccountRedirectToAccountAfterLoginAsync()
        {
            _cacheDependencyBuilderFactory.Create().AddKeys(_channelCustomSettingsRepository.GetSettingModelDependencyKeys<AccountChannelSettings>());
            return (await _channelCustomSettingsRepository.GetSettingsModel<AccountChannelSettings>()).AccountRedirectToAccountAfterLogin;
        }

        public async Task<string> GetAccountLogOutUrlAsync(string fallBackUrl)
        {
            _cacheDependencyBuilderFactory.Create().AddKeys(_channelCustomSettingsRepository.GetSettingModelDependencyKeys<AccountChannelSettings>());
            return _urlResolver.ResolveUrl((await _channelCustomSettingsRepository.GetSettingsModel<AccountChannelSettings>()).AccountLogOutUrl.AsNullOrWhitespaceMaybe().GetValueOrDefault(fallBackUrl));
        }

        public async Task<string> GetAccountResetPasswordUrlAsync(string fallBackUrl)
        {
            _cacheDependencyBuilderFactory.Create().AddKeys(_channelCustomSettingsRepository.GetSettingModelDependencyKeys<AccountChannelSettings>());
            return _urlResolver.ResolveUrl((await _channelCustomSettingsRepository.GetSettingsModel<AccountChannelSettings>()).AccountResetPassword.AsNullOrWhitespaceMaybe().GetValueOrDefault(fallBackUrl));
        }

        public async Task<string> GetAccessDeniedUrlAsync(string fallBackUrl)
        {
            _cacheDependencyBuilderFactory.Create().AddKeys(_channelCustomSettingsRepository.GetSettingModelDependencyKeys<AccountChannelSettings>());
            return _urlResolver.ResolveUrl((await _channelCustomSettingsRepository.GetSettingsModel<AccountChannelSettings>()).AccessDeniedUrl.AsNullOrWhitespaceMaybe().GetValueOrDefault(fallBackUrl));
        }

        public async Task<PasswordPolicySettings> GetPasswordPolicyAsync()
        {
            var model = await _channelCustomSettingsRepository.GetSettingsModel<MemberPasswordChannelSettings>();
            if (model.UsePasswordPolicy) {
                return new PasswordPolicySettings(model.UsePasswordPolicy, model.MinLength, model.NumNonAlphanumericChars, model.Regex.AsNullOrWhitespaceMaybe().AsNullableValue(), model.ViolationMessage.AsNullOrWhitespaceMaybe().AsNullableValue());
            }

            try {
                var passwordOptions = _identityOptions.Value.Password;
                // Convert to our basic rules
                if(!(passwordOptions.RequiredLength > 0 || passwordOptions.RequireDigit || passwordOptions.RequireLowercase || passwordOptions.RequireUppercase || passwordOptions.RequireNonAlphanumeric || passwordOptions.RequiredUniqueChars > 0)) {
                    return new PasswordPolicySettings();
                }
                // generate Regex from settings
                var regex = $"^";
                if (passwordOptions.RequiredLength > 0) {
                    regex += $"(?=.{{{passwordOptions.RequiredLength},}})";
                }
                if (passwordOptions.RequireDigit) {
                    regex += "(?=(.*\\d))";
                }
                if (passwordOptions.RequireLowercase) {
                    regex += "(?=(.*[a-z]))";
                }
                if (passwordOptions.RequireUppercase) {
                    regex += "(?=(.*[A-Z]))";
                }
                if (passwordOptions.RequireNonAlphanumeric) {
                    regex += "(?=(.*[^\\w\\s]))";
                }

                regex += ".*$";

                var passwordSettings = new PasswordPolicySettings() {
                    UsePasswordPolicy = (passwordOptions.RequiredLength > 0 || passwordOptions.RequireDigit || passwordOptions.RequireLowercase || passwordOptions.RequireUppercase || passwordOptions.RequireNonAlphanumeric || passwordOptions.RequiredUniqueChars > 0),
                    MinLength = passwordOptions.RequiredLength,
                    NumNonAlphanumericChars = passwordOptions.RequireNonAlphanumeric ? 1 : 0,
                    UniqueChars = passwordOptions.RequiredUniqueChars,
                    Regex = regex
                };
                

            } catch(Exception) {

            }

            return new PasswordPolicySettings();
        }

        public PasswordPolicySettings GetPasswordPolicy() => GetPasswordPolicyAsync().GetAwaiter().GetResult();
    }
}
