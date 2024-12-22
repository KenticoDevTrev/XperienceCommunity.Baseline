using XperienceCommunity.ChannelSettings.Attributes;

namespace XperienceCommunity.ChannelSettings.Models
{
    /// <summary>
    /// Kentico "Agnostic" Version of the settings
    /// </summary>
    public class TestSettings
    {
        
        [XperienceSettingsData("Test.Bool", false)]
        public virtual bool TestBool { get; set; } = false;

        [XperienceSettingsData("Test.Int16", 1)]
        public virtual Int16 TestInt16 { get; set; } = 1;

        [XperienceSettingsData("Test.Int32", 2)]
        public virtual Int32 TestInt32 { get; set; } = 2;

        [XperienceSettingsData("Test.Int", 3)]
        public virtual int TestInt { get; set; } = 2;

        [XperienceSettingsData("Test.Int64", 4)]
        public virtual Int64 TestInt64 { get; set; } = 3;

        [XperienceSettingsData("Test.UInt16", 5)]
        public virtual UInt16 TestUInt16 { get; set; } = 4;

        [XperienceSettingsData("Test.UInt32", 6)]
        public virtual UInt32 TestUInt32 { get; set; } = 5;

        [XperienceSettingsData("Test.UInt64", 7)]
        public virtual UInt64 TestUInt64 { get; set; } = 6;

        [XperienceSettingsData("Test.UInt64", ' ')]
        public virtual char TestChar { get; set; } = ' ';

        [XperienceSettingsData("Test.Double", 3.14)]
        public virtual double TestDouble { get; set; } = 3.14;

        [XperienceSettingsData("Test.Decimal", 1.333333)]
        public virtual decimal TestDecimal { get; set; } = 1.333333m;

        [XperienceSettingsData("Test.DateTime")]
        public virtual DateTime TestDateTime { get; set; } = DateTime.Now;

        [XperienceSettingsData("Test.Date")]
        public virtual DateTime TestDate { get; set; } = DateTime.Now.Date;

        [XperienceSettingsData("Test.String", "")]
        public virtual string TestString { get; set; } = string.Empty;

        [XperienceSettingsData("Test.Guid")]
        public virtual Guid TestGuid { get; set; }= Guid.Empty;

        [XperienceSettingsData("Test.Array")]
        public virtual int[] TestArray { get; set; } = [];

        [XperienceSettingsData("Test.Object")]
        public virtual TestName TestObject { get; set; } = new TestName();


    }

    public class TestName
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}
