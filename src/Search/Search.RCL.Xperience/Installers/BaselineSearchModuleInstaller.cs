using CMS.ContentEngine;
using CMS.ContentEngine.Internal;
using CMS.Core;
using CMS.DataEngine;
using CMS.FormEngine;
namespace Search.Installers
{
    public class BaselineSearchModuleInstaller(BaselineSearchInstallerOptions baselineSearchInstallerOptions,
        IEventLogService eventLogService,
        IInfoProvider<ContentTypeChannelInfo> contentTypeChannelInfoProvider,
        IInfoProvider<ChannelInfo> channelInfoProvider)
    {
        private readonly BaselineSearchInstallerOptions _baselineSearchInstallerOptions = baselineSearchInstallerOptions;
        private readonly IEventLogService _eventLogService = eventLogService;
        private readonly IInfoProvider<ContentTypeChannelInfo> _contentTypeChannelInfoProvider = contentTypeChannelInfoProvider;
        private readonly IInfoProvider<ChannelInfo> _channelInfoProvider = channelInfoProvider;

        public bool InstallationRan { get; set; } = false;

        public Task Install()
        {
            if (_baselineSearchInstallerOptions.AddSearchPageType) {
                CreateSearchWebpage();
            }

            InstallationRan = true;

            return Task.CompletedTask;
        }

        private void CreateSearchWebpage()
        {
            var contentItemCommonDataForm = FormHelper.GetFormInfo(ContentItemCommonDataInfo.OBJECT_TYPE, false);
            var baseMetaDataSchema = contentItemCommonDataForm.GetFormSchema("Base.Metadata");
            var redirectMetaDataSchema = contentItemCommonDataForm.GetFormSchema("Base.Redirect");
            var memberPermissionSchema = contentItemCommonDataForm.GetFormSchema("XperienceCommunity.MemberPermissionConfiguration");

            if (baseMetaDataSchema == null || redirectMetaDataSchema == null || memberPermissionSchema == null) {
                _eventLogService.LogError("BaselineModuleInstaller", "MissingSchemas", eventDescription: $"Can't create Basic page type because one of the following schemas are missing: {(baseMetaDataSchema == null ? "Base.Metadata" : "")} {(redirectMetaDataSchema == null ? "Base.Redirect" : "")} {(memberPermissionSchema == null ? "XperienceCommunity.MemberPermissionConfiguration" : "")}");
                return;
            }

            var existingSearchClass = DataClassInfoProvider.GetClasses().WhereEquals(nameof(DataClassInfo.ClassName), "Generic.Search")
                .GetEnumerableTypedResult().FirstOrDefault();
            var searchClass = existingSearchClass ?? DataClassInfo.New("Generic.Search");
            searchClass.ClassDisplayName = "Search";
            searchClass.ClassName = "Generic.Search";
            searchClass.ClassTableName = "Generic_Search";
            searchClass.ClassGUID = Guid.Parse("5ED3A9DE-5183-476E-8AC4-F1151DB3222F");
            searchClass.ClassIconClass = "xp-magnifier";
            searchClass.ClassHasUnmanagedDbSchema = false;
            searchClass.ClassType = "Content";
            searchClass.ClassContentTypeType = "Website";
            searchClass.ClassWebPageHasUrl = true;
            searchClass.ClassShortName = "GenericSearch";

            var formInfo = existingSearchClass != null ? new FormInfo(existingSearchClass.ClassFormDefinition) : FormHelper.GetBasicFormDefinition("ContentItemDataID");

            // The main field will always exist, but check Guid value to make sure stays identical.
            var existingFieldContentItemDataID = formInfo.GetFormField("ContentItemDataID");
            var guidMatches = existingFieldContentItemDataID.Guid.Equals(Guid.Parse("65eca747-4c2f-453e-9f32-3f3d11b9ddca"));
            if (!guidMatches) {
                existingFieldContentItemDataID.Guid = Guid.Parse("65eca747-4c2f-453e-9f32-3f3d11b9ddca");
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
            fieldContentItemDataCommonDataID.Guid = Guid.Parse("7f9e9d2a-bf1a-46e9-80b4-a7c01ff1f5c4");

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
            fieldContentItemDataGUID.Guid = Guid.Parse("24ff6de2-9b99-49fe-9e47-3b0091f4c2ad");

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

            searchClass.ClassFormDefinition = formInfo.GetXmlDefinition();
            if (searchClass.HasChanged) {
                DataClassInfoProvider.SetDataClassInfo(searchClass);
            }
            if (existingSearchClass == null) {
                // Also add to any web channels
                var webChannels = _channelInfoProvider.Get().WhereEquals(nameof(ChannelInfo.ChannelType), "Website").GetEnumerableTypedResult();
                foreach (var webChannel in webChannels) {
                    _contentTypeChannelInfoProvider.Set(new ContentTypeChannelInfo() {
                        ContentTypeChannelChannelID = webChannel.ChannelID,
                        ContentTypeChannelContentTypeID = searchClass.ClassID
                    });
                }
            }
        }

    }
}
