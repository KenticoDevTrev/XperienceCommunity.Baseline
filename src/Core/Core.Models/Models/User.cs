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

        /// <summary>
        /// New constructor, name is optional
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="userName"></param>
        /// <param name="userGUID"></param>
        /// <param name="email"></param>
        /// <param name="enabled"></param>
        /// <param name="isExternal"></param>
        /// <param name="isPublic"></param>
        public User(int userID, string userName, Guid userGUID, string email, bool enabled, bool isExternal, bool isPublic = false)
        {
            UserID = userID;
            UserName = userName;
            UserGUID = userGUID;
            Email = email;
            Enabled = enabled;
            IsExternal = isExternal;
            IsPublic = isPublic;
            MetaData = Maybe.None;
        }


        public Maybe<int> UserID { get; init; }
        public string UserName { get; init; }
        public Maybe<Guid> UserGUID { get; init; }

        public string Email { get; init; }

        public Maybe<string> FirstName { get; init; } = Maybe.None;
        public Maybe<string> MiddleName { get; init; } = Maybe.None;
        public Maybe<string> LastName { get; init; } = Maybe.None;
        public bool Enabled { get; init; }
        public bool IsExternal { get; init; }
        public bool IsPublic { get; init; }

        public Maybe<IUserMetadata> MetaData { get; init; } = Maybe.None;

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