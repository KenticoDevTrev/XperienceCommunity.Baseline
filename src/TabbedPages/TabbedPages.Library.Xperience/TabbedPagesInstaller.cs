using CMS.ContentEngine;
using CMS.ContentEngine.Internal;
using CMS.Core;
using CMS.DataEngine;
using CMS.FormEngine;

namespace TabbedPages
{
    public class TabbedPagesModuleInstaller(IEventLogService eventLogService,
        IInfoProvider<ContentTypeChannelInfo> contentTypeChannelInfoProvider,
        IInfoProvider<ChannelInfo> channelInfoProvider)
    {
        private readonly IEventLogService _eventLogService = eventLogService;
        private readonly IInfoProvider<ContentTypeChannelInfo> _contentTypeChannelInfoProvider = contentTypeChannelInfoProvider;
        private readonly IInfoProvider<ChannelInfo> _channelInfoProvider = channelInfoProvider;

        public bool InstallationRan { get; set; } = false;

        public Task Install()
        {
            CreateTabParentPage();
            CreateTabPage();

            InstallationRan = true;

            return Task.CompletedTask;
        }

        private void CreateTabPage()
        {
            var contentItemCommonDataForm = FormHelper.GetFormInfo(ContentItemCommonDataInfo.OBJECT_TYPE, false);
            var baseMetaDataSchema = contentItemCommonDataForm.GetFormSchema("Base.Metadata");
            var redirectMetaDataSchema = contentItemCommonDataForm.GetFormSchema("Base.Redirect");
            var memberPermissionSchema = contentItemCommonDataForm.GetFormSchema("XperienceCommunity.MemberPermissionConfiguration");

            if (baseMetaDataSchema == null || redirectMetaDataSchema == null || memberPermissionSchema == null) {
                _eventLogService.LogError("TabbedPagesModuleInstaller", "MissingSchemas", eventDescription: $"Can't create Tab Parent page type because one of the following schemas are missing: {(baseMetaDataSchema == null ? "Base.Metadata" : "")} {(redirectMetaDataSchema == null ? "Base.Redirect" : "")} {(memberPermissionSchema == null ? "XperienceCommunity.MemberPermissionConfiguration" : "")}");
                return;
            }

            var existingAccountClass = DataClassInfoProvider.GetClasses().WhereEquals(nameof(DataClassInfo.ClassName), "Generic.TabParent")
                .GetEnumerableTypedResult().FirstOrDefault();
            var accountClass = existingAccountClass ?? DataClassInfo.New("Generic.TabParent");
            accountClass.ClassDisplayName = "Tab Parent";
            accountClass.ClassName = "Generic.TabParent";
            accountClass.ClassTableName = "Generic_TabParent";
            accountClass.ClassGUID = Guid.Parse("911ED8F7-3FC4-4F2F-BEBB-F7F11BFB7346");
            accountClass.ClassIconClass = "xp-parent-child-scheme-inverted";
            accountClass.ClassHasUnmanagedDbSchema = false;
            accountClass.ClassType = "Content";
            accountClass.ClassContentTypeType = "Website";
            accountClass.ClassWebPageHasUrl = true;
            accountClass.ClassShortName = "GenericTabParent";

            var formInfo = existingAccountClass != null ? new FormInfo(existingAccountClass.ClassFormDefinition) : FormHelper.GetBasicFormDefinition("ContentItemDataID");

            // The main field will always exist, but check Guid value to make sure stays identical.
            var existingFieldContentItemDataID = formInfo.GetFormField("ContentItemDataID");
            var guidMatches = existingFieldContentItemDataID.Guid.Equals(Guid.Parse("9fb140aa-a447-4db3-80fb-a7de0483e94a"));
            if (!guidMatches) {
                existingFieldContentItemDataID.Guid = Guid.Parse("9fb140aa-a447-4db3-80fb-a7de0483e94a");
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
            fieldContentItemDataCommonDataID.Guid = Guid.Parse("5e9cd528-9a6b-4e04-aac1-a9cc4c8ad1e0");

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
            fieldContentItemDataGUID.Guid = Guid.Parse("6055e140-992c-4845-a5e1-f9c635b9458a");

            if (existingFieldContentItemDataGUID != null) {
                formInfo.UpdateFormField("ContentItemDataGUID", fieldContentItemDataGUID);
            } else {
                formInfo.AddFormItem(fieldContentItemDataGUID);
            }

            // Add the 3 schemas
            if (formInfo.GetFormSchema(baseMetaDataSchema.Guid.ToString().ToLower()) == null) {
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
            if (accountClass.HasChanged) {
                DataClassInfoProvider.SetDataClassInfo(accountClass);
            }
            if (existingAccountClass == null) {
                // Also add to any web channels
                var webChannels = _channelInfoProvider.Get().WhereEquals(nameof(ChannelInfo.ChannelType), "Website").GetEnumerableTypedResult();
                foreach (var webChannel in webChannels) {
                    _contentTypeChannelInfoProvider.Set(new ContentTypeChannelInfo() {
                        ContentTypeChannelChannelID = webChannel.ChannelID,
                        ContentTypeChannelContentTypeID = accountClass.ClassID
                    });
                }
            }
        }

        private void CreateTabParentPage()
        {
            var contentItemCommonDataForm = FormHelper.GetFormInfo(ContentItemCommonDataInfo.OBJECT_TYPE, false);
            var memberPermissionSchema = contentItemCommonDataForm.GetFormSchema("XperienceCommunity.MemberPermissionConfiguration");

            if (memberPermissionSchema == null) {
                _eventLogService.LogError("TabbedPagesModuleInstaller", "MissingSchemas", eventDescription: $"Can't create Tab page type because one of the following schemas are missing: {(memberPermissionSchema == null ? "XperienceCommunity.MemberPermissionConfiguration" : "")}");
                return;
            }

            var existingAccountClass = DataClassInfoProvider.GetClasses().WhereEquals(nameof(DataClassInfo.ClassName), "Generic.Tab")
                .GetEnumerableTypedResult().FirstOrDefault();
            var accountClass = existingAccountClass ?? DataClassInfo.New("Generic.Tab");
            accountClass.ClassDisplayName = "Tab";
            accountClass.ClassName = "Generic.Tab";
            accountClass.ClassTableName = "Generic_Tab";
            accountClass.ClassGUID = Guid.Parse("223B0055-A2D5-47A7-B391-F2BFB72E00AC");
            accountClass.ClassIconClass = "xp-tab";
            accountClass.ClassHasUnmanagedDbSchema = false;
            accountClass.ClassType = "Content";
            accountClass.ClassContentTypeType = "Website";
            accountClass.ClassWebPageHasUrl = true;
            accountClass.ClassShortName = "GenericTab";

            var formInfo = existingAccountClass != null ? new FormInfo(existingAccountClass.ClassFormDefinition) : FormHelper.GetBasicFormDefinition("ContentItemDataID");

            // The main field will always exist, but check Guid value to make sure stays identical.
            var existingFieldContentItemDataID = formInfo.GetFormField("ContentItemDataID");
            var guidMatches = existingFieldContentItemDataID.Guid.Equals(Guid.Parse("b94874b2-650f-4a71-806f-402eb83ed80f"));
            if (!guidMatches) {
                existingFieldContentItemDataID.Guid = Guid.Parse("b94874b2-650f-4a71-806f-402eb83ed80f");
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
            fieldContentItemDataCommonDataID.Guid = Guid.Parse("958aba5d-94a5-466c-85f6-0a6e3634d21b");

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
            fieldContentItemDataGUID.Guid = Guid.Parse("e9db62cd-116b-42ef-9ef5-29120e4d394b");

            if (existingFieldContentItemDataGUID != null) {
                formInfo.UpdateFormField("ContentItemDataGUID", fieldContentItemDataGUID);
            } else {
                formInfo.AddFormItem(fieldContentItemDataGUID);
            }

            // Add the 1 schema
            if (formInfo.GetFormSchema(memberPermissionSchema.Guid.ToString().ToLower()) == null) {
                var memberPermissionSchemaReference = new FormSchemaInfo() {
                    Guid = memberPermissionSchema.Guid,
                    Name = memberPermissionSchema.Guid.ToString()
                };
                formInfo.ItemsList.Add(memberPermissionSchemaReference);
            }

            accountClass.ClassFormDefinition = formInfo.GetXmlDefinition();
            if (accountClass.HasChanged) {
                DataClassInfoProvider.SetDataClassInfo(accountClass);
            }
            if (existingAccountClass == null) {
                // Also add to any web channels
                var webChannels = _channelInfoProvider.Get().WhereEquals(nameof(ChannelInfo.ChannelType), "Website").GetEnumerableTypedResult();
                foreach (var webChannel in webChannels) {
                    _contentTypeChannelInfoProvider.Set(new ContentTypeChannelInfo() {
                        ContentTypeChannelChannelID = webChannel.ChannelID,
                        ContentTypeChannelContentTypeID = accountClass.ClassID
                    });
                }
            }
        }
    }
}
