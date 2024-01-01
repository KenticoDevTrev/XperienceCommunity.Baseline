namespace Core.Models
{
    public record User : IObjectIdentifiable
    {
        public User(string userName, string firstName, string lastName, string email, bool enabled, bool isExternal, bool isPublic)
        {
            UserName = userName;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Enabled = enabled;
            IsExternal = isExternal;
            IsPublic = isPublic;
        }

        public User(int userID, string userName, Guid userGUID, string email, string firstName, string middleName, string lastName, bool enabled, bool isExternal, bool isPublic = false)
        {
            UserID = userID;
            UserName = userName;
            UserGUID = userGUID;
            Email = email;
            FirstName = firstName;
            MiddleName = middleName.AsNullOrWhitespaceMaybe();
            LastName = lastName;
            Enabled = enabled;
            IsExternal = isExternal;
            IsPublic = isPublic;
        }


        public Maybe<int> UserID { get; init; }
        public string UserName { get; init; }
        public Maybe<Guid> UserGUID { get; init; }

        public string Email { get; init; }

        public string FirstName { get; init; }
        public Maybe<string> MiddleName { get; init; }
        public string LastName { get; init; }
        public bool Enabled { get; init; }
        public bool IsExternal { get; init; }
        public bool IsPublic { get; init; }

        public ObjectIdentity ToObjectIdentity()
        {
            return new ObjectIdentity()
            {
                Id = UserID,
                CodeName = UserName,
                Guid = UserGUID
            };
        }
    }
}