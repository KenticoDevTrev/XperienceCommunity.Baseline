using CMS.Membership;
using Kentico.Membership;

namespace Core.Models
{
    public class ApplicationUserBaseline : ApplicationUser
    {
        public ApplicationUserBaseline() { }

        public int MemberId
        {
            get;
            set;
        }

        // Property that corresponds to a custom field specified in the Modules application in the admin UI
        public string? MemberFirstName
        {
            get;
            set;
        }

        public string? MemberMiddleName
        {
            get;
            set;
        }

        public string? MemberLastName
        {
            get;
            set;
        }

        public Guid? MemberGuid
        {
            get;
            set;
        }

        public override void MapToMemberInfo(MemberInfo target)
        {
            // Calls the base class implementation of the MapToMemberInfo method
            base.MapToMemberInfo(target);

            // Sets the value of the 'FirstName' MemberInfo field
            target.SetValue("MemberFirstName", MemberFirstName);
            target.SetValue("MemberMiddleName", MemberMiddleName);
            target.SetValue("MemberLastName", MemberLastName);

        }
        public override void MapFromMemberInfo(MemberInfo source)
        {
            // Calls the base class implementation of the MapFromMemberInfo method
            base.MapFromMemberInfo(source);

            // Maps the 'MemberId' property to the extended member object
            MemberId = source.MemberID;
            MemberGuid = source.MemberGuid;

            // Sets the value of the 'FirstName' and 'LastName' property
            MemberFirstName = source.GetValue<string?>("MemberFirstName", null);
            MemberMiddleName = source.GetValue<string?>("MemberMiddleName", null);
            MemberLastName = source.GetValue<string?>("MemberLastName", null);
        }
    }
}
