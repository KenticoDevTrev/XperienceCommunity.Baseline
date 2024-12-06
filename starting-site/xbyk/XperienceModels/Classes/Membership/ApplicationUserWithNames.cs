using CMS.Membership;
using Kentico.Membership;

namespace XperienceModels.Classes.Membership
{
    public class ApplicationUserWithNames : ApplicationUser
    {
        public ApplicationUserWithNames() { }

        public int MemberId
        {
            get;
            set;
        }

        // Property that corresponds to a custom field specified in the Modules application in the admin UI
        public string? FirstName
        {
            get;
            set;
        }

        public string? LastName
        {
            get;
            set;
        }

        public override void MapToMemberInfo(MemberInfo target)
        {
            // Calls the base class implementation of the MapToMemberInfo method
            base.MapToMemberInfo(target);

            // Maps the 'MemberId' property to the extended member object
            target.MemberID = MemberId;

            // Sets the value of the 'FirstName' MemberInfo field
            target.SetValue("MemberFirstName", FirstName);
            target.SetValue("MemberLastName", FirstName);
        }
        public override void MapFromMemberInfo(MemberInfo source)
        {
            // Calls the base class implementation of the MapFromMemberInfo method
            base.MapFromMemberInfo(source);

            // Maps the 'MemberId' property to the extended member object
            MemberId = source.MemberID;

            // Sets the value of the 'FirstName' and 'LastName' property
            FirstName = source.GetValue<string?>("MemberFirstName", null);
            LastName = source.GetValue<string?>("MemberLastName", null);
        }
    }
}
