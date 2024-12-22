using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;
using Navigation.Models;

namespace Account.Installers
{
    public class BaselineNavigationModuleInstaller(BaselineNavigationOptions baselineNavigationOptions,
        IEventLogService eventLogService,
        IInfoProvider<ContentTypeChannelInfo> contentTypeChannelInfoProvider,
        IInfoProvider<ChannelInfo> channelInfoProvider)
    {
        private readonly BaselineNavigationOptions _baselineNavigationOptions = baselineNavigationOptions;
        private readonly IEventLogService _eventLogService = eventLogService;
        private readonly IInfoProvider<ContentTypeChannelInfo> _contentTypeChannelInfoProvider = contentTypeChannelInfoProvider;
        private readonly IInfoProvider<ChannelInfo> _channelInfoProvider = channelInfoProvider;

        public bool InstallationRan { get; set; } = false;

        public void Install()
        {
            CreateNavigationWebPage();
            InstallationRan = true;
        }

        private void CreateNavigationWebPage()
        {
            // TODO:
            /*
            var contentItemCommonDataForm = FormHelper.GetFormInfo(ContentItemCommonDataInfo.OBJECT_TYPE, false);
            var baseMetaDataSchema = contentItemCommonDataForm.GetFormSchema("Base.Metadata");
            var redirectMetaDataSchema = contentItemCommonDataForm.GetFormSchema("Base.Redirect");
            var memberPermissionSchema = contentItemCommonDataForm.GetFormSchema("XperienceCommunity.MemberPermissionConfiguration");

            if(baseMetaDataSchema == null || redirectMetaDataSchema == null || memberPermissionSchema == null) {
                _eventLogService.LogError("BaselineAccountModuleInstaller", "MissingSchemas", eventDescription: $"Can't create Account page type because one of the following schemas are missing: {(baseMetaDataSchema == null ? "Base.Metadata" : "")} {(redirectMetaDataSchema == null ? "Base.Redirect" : "")} {(memberPermissionSchema == null ? "XperienceCommunity.MemberPermissionConfiguration" : "")}");
                return;
            }

            var existingAccountClass = DataClassInfoProvider.GetClasses().WhereEquals(nameof(DataClassInfo.ClassName), "Generic.Account")
                .GetEnumerableTypedResult().FirstOrDefault();
            var accountClass = existingAccountClass ?? DataClassInfo.New("Generic.Account");
            accountClass.ClassDisplayName = "Account";
            accountClass.ClassName = "Generic.Account";
            accountClass.ClassTableName = "Generic_Account";
            accountClass.ClassGUID = Guid.Parse("BA6B28F3-AC79-4EEA-98BF-DDBD13410C41");
            accountClass.ClassIconClass = "xp-users";
            accountClass.ClassHasUnmanagedDbSchema = false;
            accountClass.ClassType = "Content";
            accountClass.ClassContentTypeType = "Website";
            accountClass.ClassWebPageHasUrl = true;
            accountClass.ClassShortName = "GenericAccount";

            var formInfo = existingAccountClass != null ? new FormInfo(existingAccountClass.ClassFormDefinition) : FormHelper.GetBasicFormDefinition("ContentItemDataID");
            
            // The main field will always exist, but check Guid value to make sure stays identical.
            var existingFieldContentItemDataID = formInfo.GetFormField("ContentItemDataID");
            var guidMatches = existingFieldContentItemDataID.Guid.Equals(Guid.Parse("45030d68-1a28-481a-a322-4c1986a76970"));
            if (!guidMatches) {
                existingFieldContentItemDataID.Guid = Guid.Parse("45030d68-1a28-481a-a322-4c1986a76970");
                formInfo.UpdateFormField("ContentItemDataID", existingFieldContentItemDataID);
            }

            // Content Item Data Common Data ID Field
            var existingFieldContentItemDataCommonDataID = formInfo.GetFormField("ContentItemDataCommonDataID");
            var fieldContentItemDataCommonDataID = existingFieldContentItemDataCommonDataID ?? new FormFieldInfo();
            fieldContentItemDataCommonDataID.Name = "ContentItemDataCommonDataID";
            fieldContentItemDataCommonDataID.AllowEmpty = false;
            fieldContentItemDataCommonDataID.DataType = "integer";
            fieldContentItemDataCommonDataID.Enabled = true;
            fieldContentItemDataCommonDataID.Visible = false;
            
            fieldContentItemDataCommonDataID.ReferenceToObjectType = "cms.contentitemcommondata";
            fieldContentItemDataCommonDataID.ReferenceType = ObjectDependencyEnum.Required;
            fieldContentItemDataCommonDataID.System = true;
            fieldContentItemDataCommonDataID.Guid = Guid.Parse("0029ce15-2574-483d-99f2-a1e882fab247");

            if (existingFieldContentItemDataCommonDataID != null) {
                formInfo.UpdateFormField("ContentItemDataCommonDataID", fieldContentItemDataCommonDataID);
            } else {
                formInfo.AddFormItem(fieldContentItemDataCommonDataID);
            }

            // Content Item Data Common Data ID Field
            var existingFieldContentItemDataGUID = formInfo.GetFormField("ContentItemDataGUID");
            var fieldContentItemDataGUID = existingFieldContentItemDataGUID ?? new FormFieldInfo();
            fieldContentItemDataGUID.Name = "ContentItemDataGUID";
            fieldContentItemDataGUID.AllowEmpty = false;
            fieldContentItemDataGUID.DataType = "guid";
            fieldContentItemDataGUID.Enabled = true;
            fieldContentItemDataGUID.Visible = false;
            fieldContentItemDataGUID.System = true;
            fieldContentItemDataGUID.IsUnique = true;
            fieldContentItemDataGUID.IsDummyFieldFromMainForm = true;
            fieldContentItemDataGUID.Guid = Guid.Parse("2eb9366b-88d9-4040-8e58-9a33706ab8e7");

            if (existingFieldContentItemDataGUID != null) {
                formInfo.UpdateFormField("ContentItemDataGUID", fieldContentItemDataGUID);
            } else {
                formInfo.AddFormItem(fieldContentItemDataGUID);
            }

            // Just add the 3 schemas
            if(formInfo.GetFormSchema(baseMetaDataSchema.Guid.ToString().ToLower()) == null) {
                var baseMetaDataSchemaReference = new FormSchemaInfo() {
                    Guid = baseMetaDataSchema.Guid,
                    Name = baseMetaDataSchema.Guid.ToString()
                };
                formInfo.ItemsList.Add(baseMetaDataSchemaReference);
            }
            if (formInfo.GetFormSchema(redirectMetaDataSchema.Guid.ToString().ToLower()) == null) {
                var redirectMetaDataSchemaReference = new FormSchemaInfo() {
                    Guid = redirectMetaDataSchema.Guid,
                    Name = redirectMetaDataSchema.Guid.ToString()
                };
                formInfo.ItemsList.Add(redirectMetaDataSchemaReference);

            }
            if (formInfo.GetFormSchema(memberPermissionSchema.Guid.ToString().ToLower()) == null) {
                var memberPermissionSchemaReference = new FormSchemaInfo() {
                    Guid = memberPermissionSchema.Guid,
                    Name = memberPermissionSchema.Guid.ToString()
                };
                formInfo.ItemsList.Add(memberPermissionSchemaReference);
            }

            accountClass.ClassFormDefinition = formInfo.GetXmlDefinition();
            if(accountClass.HasChanged) {
                DataClassInfoProvider.SetDataClassInfo(accountClass);
            }
            if(existingAccountClass == null) {
                // Also add to any web channels
                var webChannels = _channelInfoProvider.Get().WhereEquals(nameof(ChannelInfo.ChannelType), "Website").GetEnumerableTypedResult();
                foreach (var webChannel in webChannels) {
                    _contentTypeChannelInfoProvider.Set(new ContentTypeChannelInfo() {
                        ContentTypeChannelChannelID = webChannel.ChannelID,
                        ContentTypeChannelContentTypeID = accountClass.ClassID
                    });
                }
            }
            */
        }

    }
}
