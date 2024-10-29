using CMS.Membership;

namespace Core.Services.Implementation
{
    public class UserMetadataProvider : IUserMetadataProvider
    {
        public Task<Maybe<IUserMetadata>> GetUserMetadata(MemberInfo memberInfo, User user)
        {
            return Task.FromResult(Maybe<IUserMetadata>.None);
        }
    }
}
