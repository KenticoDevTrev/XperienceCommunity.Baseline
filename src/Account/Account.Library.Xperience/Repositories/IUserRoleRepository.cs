using Core.Models;

namespace Account.Repositories
{
    public interface IUserRoleRepository
    {
        /// <summary>
        /// Optimized Role retrieval
        /// </summary>
        /// <typeparam name="TUser"></typeparam>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<IEnumerable<ObjectIdentityFilled>> GetUserRoles<TUser>(TUser user) where TUser : User;

        /// <summary>
        /// Optimized Role Retrieval.  For this, the "CodeName" must be the MemberName (username), not email
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<IEnumerable<ObjectIdentityFilled>> GetUserRoles(ObjectIdentity user);
    }
}
