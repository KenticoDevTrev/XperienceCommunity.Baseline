using CMS.Membership;

namespace Core.Services
{
    public interface IUserMetadataProvider
    {
        /// <summary>
        /// Use this to add custom fields or data to the User object (https://docs.kentico.com/developers-and-admins/development/registration-and-authentication/add-fields-to-member-objects#add-the-new-fields-to-the-memberinfo-object)
        /// </summary>
        /// <param name="memberInfo">The MemberInfo (should contain your extra fields if you extend it)</param>
        /// <param name="user">The User object, for reference only</param>
        /// <returns>an IUserMetadata of your creation.</returns>
        Task<Maybe<IUserMetadata>> GetUserMetadata(MemberInfo memberInfo, User user);
    }
}
