using Kentico.Membership;

namespace Core.Services.Implementations
{
    // Base implementation, you can easily implement your own though to extend both the ApplicationUser and the Generic Model to add properties
    public class BaselineUserMapper<TUser> : IBaselineUserMapper<TUser> where TUser : ApplicationUser, new()
    {
        public Task<TUser> ToApplicationUser(User user)
        {
            var appUser = new TUser() {
                UserName = user.UserName,
                Enabled = user.Enabled,
                Email = user.Email,
                IsExternal = user.IsExternal,
                SecurityStamp = user.SecurityStamp
            };
            if (user.UserID.TryGetValue(out var userId)) {
                appUser.Id = userId;
            }

            // Put your own custom application user type here, and possibly also your own generic user type
            if (appUser is ApplicationUserBaseline customApplicationUser) {
                customApplicationUser.MemberFirstName = user.FirstName.AsNullableValue();
                customApplicationUser.MemberMiddleName = user.MiddleName.AsNullableValue();
                customApplicationUser.MemberLastName = user.LastName.AsNullableValue();
                customApplicationUser.MemberId = user.UserID.GetValueOrDefault(0);
                customApplicationUser.MemberGuid = user.UserGUID.GetValueOrDefault(Guid.Empty);
                /* Here is an example of using Custom MetaData on your User object to pass back to the Applicationuser. */
                /*
                if (customGenericUser.MetaData.TryGetValue(out var metaData) && metaData is CustomUserPreferencesMetaData userPreferences) { 
                    customApplicationUser.PreferredLanguage = userPreferences.PreferredLanguage;
                    customApplicationUser.FavoriteColor = userPreferences.FavoriteColor;
                }
                */

                // Don't know why but required an additional type check...
                if (customApplicationUser is TUser appUserBackToNormal) {
                    return Task.FromResult(appUserBackToNormal);
                }
            }

            return Task.FromResult(appUser);
        }

        public Task<User> ToUser(TUser applicationUser)
        {
            var user = new User() {
                UserID = applicationUser.Id,
                UserName = applicationUser.UserName ?? "public",
                Email = applicationUser.Email ?? "public@localhost",
                Enabled = applicationUser.Enabled,
                IsExternal = applicationUser.IsExternal,
                IsPublic = (applicationUser.UserName ?? "public").Equals("public", StringComparison.OrdinalIgnoreCase),
                SecurityStamp = applicationUser.SecurityStamp ?? string.Empty
            };

            // Put your own custom user type here
            if (applicationUser is ApplicationUserBaseline appUserBaseline) {
                user = user with {
                    FirstName = appUserBaseline.MemberFirstName.AsNullOrWhitespaceMaybe(),
                    MiddleName = appUserBaseline.MemberMiddleName.AsNullOrWhitespaceMaybe(),
                    LastName = appUserBaseline.MemberLastName.AsNullOrWhitespaceMaybe(),
                    UserGUID = appUserBaseline.MemberGuid.GetValueOrDefault(Guid.Empty)
                };

                /* Here is an example of using Custom MetaData passed from your custom ApplicationUser (with custom fields from MemberInfo) to the MetaData */
                /*
                user = user with {
                    MetaData = new CustomUserPreferencesMetaData() {
                        PreferredLanguage = "en",
                        FavoriteColor = "blue"
                    }
                };
                */

                // You can customize this on your own implementations
                return Task.FromResult(user);
            }

            return Task.FromResult(user);
        }
    }
}
