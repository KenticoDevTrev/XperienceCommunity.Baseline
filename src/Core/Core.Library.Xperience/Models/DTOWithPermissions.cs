using XperienceCommunity.MemberRoles.Interfaces;

namespace Core.Models
{
    public record DTOWithPermissions<T>(T Model, bool MemberPermissionOverride, int ContentID, bool MemberPermissionIsSecure, string[] MemberPermissionRoleTags) : IMemberPermissionConfiguration
    {
        public bool GetCheckPermissions() => true;

        public int GetContentID() => ContentID;

        public bool GetMemberPermissionIsSecure() => MemberPermissionIsSecure;

        public bool GetMemberPermissionOverride() => MemberPermissionOverride;

        public IEnumerable<string> GetMemberPermissionRoleTags() => MemberPermissionRoleTags;
    }
}
