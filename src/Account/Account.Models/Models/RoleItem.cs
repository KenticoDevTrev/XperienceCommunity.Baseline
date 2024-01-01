namespace Account.Models
{
    public record RoleItem : IObjectIdentifiable
    {
        public RoleItem(ObjectIdentity siteID, int roleID, string roleDisplayName, string roleName, Guid roleGUID)
        {
            Site = siteID;
            RoleID = roleID;
            RoleDisplayName = roleDisplayName;
            RoleName = roleName;
            RoleGUID = roleGUID;
        }

        public ObjectIdentity Site { get; init; }
        public int RoleID { get; init; }
        public string RoleDisplayName { get; init; }
        public string RoleName { get; init; }
        public Maybe<string> RoleDescription { get; init; }
        public Guid RoleGUID { get; init; }
        public bool RoleIsDomain { get; init; } = false;

        public ObjectIdentity ToObjectIdentity()
        {
            return new ObjectIdentity()
            {
                CodeName = RoleName,
                Guid = RoleGUID,
                Id = RoleID
            };
        }
    }
}
