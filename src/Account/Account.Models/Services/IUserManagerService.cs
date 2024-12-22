namespace Account.Services
{
    /// <summary>
    /// Wrapper around the UserManager, since the Type will change depending on implementation
    /// </summary>
    public interface IUserManagerService
    {
        Task<bool> CheckPasswordByNameAsync(string userName, string password);
        Task<bool> CheckPasswordByEmailAsync(string email, string password);
        Task<bool> CheckPasswordByIdAsync(string userId, string password);
        Task<bool> CheckPasswordByLoginAsync(string loginProvider, string providerKey, string password);

        Task<string> GenerateTwoFactorTokenByNameAsync(string userName, string tokenProvider);
        Task<string> GenerateTwoFactorTokenByEmailAsync(string email, string tokenProvider);
        Task<string> GenerateTwoFactorTokenByIdAsync(string userId, string tokenProvider);
        Task<string> GenerateTwoFactorTokenByLoginAsync(string loginProvider, string providerKey, string tokenProvider);

        Task<bool> UserExistsByNameAsync(string userName);
        Task<bool> UserExistsByEmailAsync(string email);
        Task<bool> UserExistsByIdAsync(string userId);
        Task<bool> UserExistsByLoginAsync(string loginProvider, string providerKey);

        Task<int> GetUserIDByNameAsync(string userName);
        Task<int> GetUserIDByEmailAsync(string email);
        Task<int> GetUserIDByIdAsync(string userId);
        Task<int> GetUserIDByLoginAsync(string loginProvider, string providerKey);

        Task<bool> VerifyTwoFactorTokenByNameAsync(string userName, string tokenProvider, string twoFormCode);
        Task<bool> VerifyTwoFactorTokenByEmailAsync(string email, string tokenProvider, string twoFormCode);
        Task<bool> VerifyTwoFactorTokenByIdAsync(string userId, string tokenProvider, string twoFormCode);
        Task<bool> VerifyTwoFactorTokenByLoginAsync(string loginProvider, string providerKey, string tokenProvider, string twoFormCode);

        Task<bool> EnableUserByNameAsync(string userName, bool setAsExternalIfExternal);
        Task<bool> EnableUserByEmailAsync(string email, bool setAsExternalIfExternal);
        Task<bool> EnableUserByIdAsync(string UserId, bool setAsExternalIfExternal);
        Task<bool> EnableUserByLoginAsync(string loginProvider, string providerKey, bool setAsExternalIfExternal);

        Task<string> GetSecurityStampAsync(string userName);
    }
}
