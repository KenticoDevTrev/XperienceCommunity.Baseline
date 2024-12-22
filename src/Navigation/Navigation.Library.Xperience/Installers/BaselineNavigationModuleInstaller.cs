using CMS.ContentEngine;
using CMS.ContentEngine.Internal;
using CMS.Core;
using CMS.DataEngine;
using CMS.FormEngine;
using Navigation.Models;
using NavigationType = Generic.Navigation;
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
            var contentItemCommonDataForm = FormHelper.GetFormInfo(ContentItemCommonDataInfo.OBJECT_TYPE, false);
            var memberPermissionSchema = contentItemCommonDataForm.GetFormSchema("XperienceCommunity.MemberPermissionConfiguration");

            if(memberPermissionSchema == null) {
                _eventLogService.LogError("BaselineNavigationModuleInstaller", "MissingSchemas", eventDescription: $"Can't create Navigation page type because one of the following schemas are missing: {(memberPermissionSchema == null ? "XperienceCommunity.MemberPermissionConfiguration" : "")}");
                return;
            }

            var existingClassNavigation = DataClassInfoProvider.GetClasses().WhereEquals(nameof(DataClassInfo.ClassName), Generic.Navigation.CONTENT_TYPE_NAME)
        .GetEnumerableTypedResult().FirstOrDefault();
            var classNavigation = existingClassNavigation ?? DataClassInfo.New(Generic.Navigation.CONTENT_TYPE_NAME);
            classNavigation.ClassDisplayName = "Navigation";
            classNavigation.ClassName = "Generic.Navigation";
            classNavigation.ClassTableName = "Generic_Navigation";
            classNavigation.ClassGUID = Guid.Parse("179434D6-683B-4DE8-B19F-0130C7759BB5");
            classNavigation.ClassIconClass = "xp-menu";
            classNavigation.ClassHasUnmanagedDbSchema = false;
            classNavigation.ClassType = "Content";
            classNavigation.ClassContentTypeType = "Website";
            classNavigation.ClassWebPageHasUrl = true;
            classNavigation.ClassShortName = "GenericNavigation";

            var formInfo = existingClassNavigation != null ? new FormInfo(existingClassNavigation.ClassFormDefinition) : FormHelper.GetBasicFormDefinition("ContentItemDataID");

            // The main field will always exist, but check Guid value to make sure stays identical.
            var existingFieldContentItemDataID = formInfo.GetFormField("ContentItemDataID");
            var guidMatches = existingFieldContentItemDataID.Guid.Equals(Guid.Parse("1ee75485-8c25-4c72-96ac-b69d1ff20099"));
            if (!guidMatches) {
                existingFieldContentItemDataID.Guid = Guid.Parse("1ee75485-8c25-4c72-96ac-b69d1ff20099");
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
            fieldContentItemDataCommonDataID.Guid = Guid.Parse("afff899c-d470-4004-9170-35ab18fe1144");

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
            fieldContentItemDataGUID.Guid = Guid.Parse("fda5a1f4-00be-4eae-9dfc-ae7d26d95071");

            if (existingFieldContentItemDataGUID != null) {
                formInfo.UpdateFormField("ContentItemDataGUID", fieldContentItemDataGUID);
            } else {
                formInfo.AddFormItem(fieldContentItemDataGUID);
            }


            // Add or Update Title Field
            var existingFieldTitle = contentItemCommonDataForm.GetFormField(nameof(NavigationType.NavigationType));
            var fieldTitle = existingFieldTitle ?? new FormFieldInfo();
            fieldTitle.Name = nameof(NavigationType.NavigationType);
            fieldTitle.AllowEmpty = false;
            fieldTitle.Precision = 0;
            fieldTitle.Size = 10;
            fieldTitle.DataType = "text";
            fieldTitle.Enabled = true;
            fieldTitle.Visible = true;
            fieldTitle.Guid = Guid.Parse("3d1d9d6b-1d41-467a-8660-a1269f33e1cd");
            fieldTitle.SetComponentName("Kentico.Administration.DropDownSelector");
            fieldTitle.Settings.Add("Options", "automatic;Automatic\nmanual;Manual");
            fieldTitle.Settings.Add("OptionsValueSeparator", ";");
            fieldTitle.Settings.Add("Placeholder", "automatic");
            fieldTitle.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, "Navigation Type");
            fieldTitle.SetPropertyValue(FormFieldPropertyEnum.DefaultValue, "automatic");
            fieldTitle.SetPropertyValue(FormFieldPropertyEnum.FieldDescription, "Whether or not the Navigation Item should automatically build itself off of the selected page, or if you want to manually enter in the information.");
            fieldTitle.SetPropertyValue(FormFieldPropertyEnum.FieldDescriptionAsHtml, "False");
            fieldTitle.SetPropertyValue(FormFieldPropertyEnum.ExplanationText, "Automatic = Selected page. Manual = Using Navigation Text and URL");
            fieldTitle.SetPropertyValue(FormFieldPropertyEnum.ExplanationTextAsHtml, "False");

            if (existingFieldTitle != null) {
                contentItemCommonDataForm.UpdateFormField(nameof(NavigationType.NavigationType), fieldTitle);
            } else {
                contentItemCommonDataForm.AddFormItem(fieldTitle);
            }

            // Add or Update Description Field
            var existingFieldDescription = contentItemCommonDataForm.GetFormField(nameof(NavigationType.NavigationWebPageItemGuid));
            var fieldDescription = existingFieldDescription ?? new FormFieldInfo();
            fieldDescription.Name = nameof(NavigationType.NavigationWebPageItemGuid);
            fieldDescription.AllowEmpty = true;
            fieldDescription.Precision = 0;
            fieldDescription.DataType = "webpages";
            fieldDescription.Enabled = true;
            fieldDescription.Visible = true;
            fieldDescription.Guid = Guid.Parse("abd80753-a218-4382-b245-30d1adb8064e");
            fieldDescription.SetComponentName("Kentico.Administration.WebPageSelector");
            fieldTitle.Settings.Add("MaximumPages", "1");
            fieldTitle.Settings.Add("Sortable", "False");
            fieldDescription.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, "Page");
            fieldDescription.SetPropertyValue(FormFieldPropertyEnum.FieldDescription, "Can select a page, the navigation will be based on this page's WebPageItemName and Url");
            fieldDescription.SetPropertyValue(FormFieldPropertyEnum.FieldDescriptionAsHtml, "False");
            fieldDescription.SetPropertyValue(FormFieldPropertyEnum.ExplanationTextAsHtml, "False");

            if (existingFieldDescription != null) {
                contentItemCommonDataForm.UpdateFormField(nameof(NavigationType.NavigationWebPageItemGuid), fieldDescription);
            } else {
                contentItemCommonDataForm.AddFormItem(fieldDescription);
            }

            // Add or Update LinkText Field
            var existingFieldLinkText = contentItemCommonDataForm.GetFormField(nameof(NavigationType.NavigationLinkText));
            var fieldLinkText = existingFieldLinkText ?? new FormFieldInfo();
            fieldLinkText.Name = nameof(NavigationType.NavigationLinkText);
            fieldLinkText.AllowEmpty = true;
            fieldLinkText.Precision = 0;
            fieldLinkText.DataType = "longtext";
            fieldLinkText.Enabled = true;
            fieldLinkText.Visible = true;
            fieldLinkText.Guid = Guid.Parse("b7e9ec6e-bab6-48b6-a069-3f51819adea1");
            fieldLinkText.SetComponentName("Kentico.Administration.TextArea");
            fieldLinkText.Settings.Add("CopyButtonVisible", "False");
            fieldLinkText.Settings.Add("MaxRowsNumber", "5");
            fieldLinkText.Settings.Add("MinRowsNumber", "1");
            fieldLinkText.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, "Link Text");
            fieldLinkText.SetPropertyValue(FormFieldPropertyEnum.FieldDescription, "Can be HTML (icons and stuff), but be sure to make it valid HTML if you do.");
            fieldLinkText.SetPropertyValue(FormFieldPropertyEnum.FieldDescriptionAsHtml, "False");
            fieldLinkText.SetPropertyValue(FormFieldPropertyEnum.ExplanationTextAsHtml, "False");
            fieldLinkText.VisibilityConditionConfigurationXmlData = "<VisibilityConditionConfiguration><Identifier>Kentico.Administration.IsEqualToString</Identifier><Properties><PropertyName>NavigationType</PropertyName><Value>manual</Value><CaseSensitive>true</CaseSensitive></Properties></VisibilityConditionConfiguration>";

            if (existingFieldLinkText != null) {
                contentItemCommonDataForm.UpdateFormField(nameof(NavigationType.NavigationLinkText), fieldLinkText);
            } else {
                contentItemCommonDataForm.AddFormItem(fieldLinkText);
            }

            // Add or Update LinkUrl Field
            var existingFieldLinkUrl = contentItemCommonDataForm.GetFormField(nameof(NavigationType.NavigationLinkUrl));
            var fieldLinkUrl = existingFieldLinkUrl ?? new FormFieldInfo();
            fieldLinkUrl.Name = nameof(NavigationType.NavigationLinkUrl);
            fieldLinkUrl.AllowEmpty = true;
            fieldLinkUrl.Precision = 0;
            fieldLinkUrl.Size = 250;
            fieldLinkUrl.DataType = "text";
            fieldLinkUrl.Enabled = true;
            fieldLinkUrl.Visible = true;
            fieldLinkUrl.Guid = Guid.Parse("b8fd90f9-580c-4a3d-84d4-717bb8220602");
            fieldLinkUrl.SetComponentName("Kentico.Administration.TextInput");
            fieldLinkUrl.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, "Link");
            fieldLinkUrl.SetPropertyValue(FormFieldPropertyEnum.FieldDescription, "Nav Link");
            fieldLinkUrl.SetPropertyValue(FormFieldPropertyEnum.FieldDescriptionAsHtml, "False");
            fieldLinkUrl.SetPropertyValue(FormFieldPropertyEnum.ExplanationTextAsHtml, "False");
            fieldLinkUrl.VisibilityConditionConfigurationXmlData = "<VisibilityConditionConfiguration><Identifier>Kentico.Administration.IsEqualToString</Identifier><Properties><PropertyName>NavigationType</PropertyName><Value>manual</Value><CaseSensitive>true</CaseSensitive></Properties></VisibilityConditionConfiguration>";

            if (existingFieldLinkUrl != null) {
                contentItemCommonDataForm.UpdateFormField(nameof(NavigationType.NavigationLinkUrl), fieldLinkUrl);
            } else {
                contentItemCommonDataForm.AddFormItem(fieldLinkUrl);
            }

            // Add or Update LinkTarget Field
            var existingFieldLinkTarget = contentItemCommonDataForm.GetFormField(nameof(NavigationType.NavigationLinkTarget));
            var fieldLinkTarget = existingFieldLinkTarget ?? new FormFieldInfo();
            fieldLinkTarget.Name = nameof(NavigationType.NavigationLinkTarget);
            fieldLinkTarget.AllowEmpty = false;
            fieldLinkTarget.Precision = 0;
            fieldLinkTarget.Size = 25;
            fieldLinkTarget.DataType = "text";
            fieldLinkTarget.Enabled = true;
            fieldLinkTarget.Visible = true;
            fieldLinkTarget.Guid = Guid.Parse("b446d1a8-c5e6-4812-84db-bd6b133c7a3e");
            fieldLinkTarget.SetComponentName("Kentico.Administration.DropDownSelector");
            fieldLinkTarget.Settings.Add("Options", "_self;Open in this window\n_blank;Open in a new window");
            fieldLinkTarget.Settings.Add("OptionsValueSeparator", ";");
            fieldLinkTarget.Settings.Add("Placeholder", "_self");
            fieldLinkTarget.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, "Link Target");
            fieldLinkTarget.SetPropertyValue(FormFieldPropertyEnum.DefaultValue, "_self");
            fieldLinkTarget.SetPropertyValue(FormFieldPropertyEnum.FieldDescriptionAsHtml, "False");
            fieldLinkTarget.SetPropertyValue(FormFieldPropertyEnum.ExplanationTextAsHtml, "False");
            fieldLinkTarget.VisibilityConditionConfigurationXmlData = "<VisibilityConditionConfiguration><Identifier>Kentico.Administration.IsEqualToString</Identifier><Properties><PropertyName>NavigationType</PropertyName><Value>manual</Value><CaseSensitive>true</CaseSensitive></Properties></VisibilityConditionConfiguration>";

            if (existingFieldLinkTarget != null) {
                contentItemCommonDataForm.UpdateFormField(nameof(NavigationType.NavigationLinkTarget), fieldLinkTarget);
            } else {
                contentItemCommonDataForm.AddFormItem(fieldLinkTarget);
            }

            // Add or Update IsMegaMenu Field
            var existingFieldIsMegaMenu = contentItemCommonDataForm.GetFormField(nameof(NavigationType.NavigationIsMegaMenu));
            var fieldIsMegaMenu = existingFieldIsMegaMenu ?? new FormFieldInfo();
            fieldIsMegaMenu.Name = nameof(NavigationType.NavigationIsMegaMenu);
            fieldIsMegaMenu.AllowEmpty = false;
            fieldIsMegaMenu.Precision = 0;
            fieldIsMegaMenu.DataType = "boolean";
            fieldIsMegaMenu.Enabled = true;
            fieldIsMegaMenu.Visible = true;
            fieldIsMegaMenu.Guid = Guid.Parse("af760f1f-c280-426f-83ef-8b8f5aa63043");
            fieldIsMegaMenu.SetComponentName("Kentico.Administration.Checkbox");
            fieldIsMegaMenu.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, "Is Mega Menu");
            fieldIsMegaMenu.SetPropertyValue(FormFieldPropertyEnum.FieldDescription, "If true, then the navigation will render the Page contents within the mega menu. Typically mega menu pages will not have any child navigation elements.");
            fieldIsMegaMenu.SetPropertyValue(FormFieldPropertyEnum.FieldDescriptionAsHtml, "False");
            fieldIsMegaMenu.SetPropertyValue(FormFieldPropertyEnum.ExplanationTextAsHtml, "False");

            if (existingFieldIsMegaMenu != null) {
                contentItemCommonDataForm.UpdateFormField(nameof(NavigationType.NavigationIsMegaMenu), fieldIsMegaMenu);
            } else {
                contentItemCommonDataForm.AddFormItem(fieldIsMegaMenu);
            }

            // Add or Update LinkAlt Field
            var existingFieldLinkAlt = contentItemCommonDataForm.GetFormField(nameof(NavigationType.NavigationLinkAlt));
            var fieldLinkAlt = existingFieldLinkAlt ?? new FormFieldInfo();
            fieldLinkAlt.Name = nameof(NavigationType.NavigationLinkAlt);
            fieldLinkAlt.AllowEmpty = true;
            fieldLinkAlt.Precision = 0;
            fieldLinkAlt.Size = 200;
            fieldLinkAlt.DataType = "text";
            fieldLinkAlt.Enabled = true;
            fieldLinkAlt.Visible = true;
            fieldLinkAlt.Guid = Guid.Parse("cbbb3bc8-ed57-4f2f-8647-46a130c1d9c7");
            fieldLinkAlt.SetComponentName("Kentico.Administration.TextInput");
            fieldLinkAlt.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, "Alternative Text");
            fieldLinkAlt.SetPropertyValue(FormFieldPropertyEnum.FieldDescription, "Descriptive text for on hover and for screen readers");
            fieldLinkAlt.SetPropertyValue(FormFieldPropertyEnum.FieldDescriptionAsHtml, "False");
            fieldLinkAlt.SetPropertyValue(FormFieldPropertyEnum.ExplanationTextAsHtml, "False");

            if (existingFieldLinkAlt != null) {
                contentItemCommonDataForm.UpdateFormField(nameof(NavigationType.NavigationLinkAlt), fieldLinkAlt);
            } else {
                contentItemCommonDataForm.AddFormItem(fieldLinkAlt);
            }

            // Add or Update LinkOnClick Field
            var existingFieldLinkOnClick = contentItemCommonDataForm.GetFormField(nameof(NavigationType.NavigationLinkOnClick));
            var fieldLinkOnClick = existingFieldLinkOnClick ?? new FormFieldInfo();
            fieldLinkOnClick.Name = nameof(NavigationType.NavigationLinkOnClick);
            fieldLinkOnClick.AllowEmpty = true;
            fieldLinkOnClick.Precision = 0;
            fieldLinkOnClick.DataType = "longtext";
            fieldLinkOnClick.Enabled = true;
            fieldLinkOnClick.Visible = true;
            fieldLinkOnClick.Guid = Guid.Parse("f575cf48-b76b-4c43-9238-34740a436da5");
            fieldLinkOnClick.SetComponentName("Kentico.Administration.CodeEditor");
            fieldLinkOnClick.Settings.Add("Language", "javascript");
            fieldLinkOnClick.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, "On Click (Javascript)");
            fieldLinkOnClick.SetPropertyValue(FormFieldPropertyEnum.FieldDescription, "Optional On click for the link.");
            fieldLinkOnClick.SetPropertyValue(FormFieldPropertyEnum.FieldDescriptionAsHtml, "False");
            fieldLinkOnClick.SetPropertyValue(FormFieldPropertyEnum.ExplanationTextAsHtml, "False");

            if (existingFieldLinkOnClick != null) {
                contentItemCommonDataForm.UpdateFormField(nameof(NavigationType.NavigationLinkOnClick), fieldLinkOnClick);
            } else {
                contentItemCommonDataForm.AddFormItem(fieldLinkOnClick);
            }

            // Add or Update LinkCSS Field
            var existingFieldLinkCSS = contentItemCommonDataForm.GetFormField(nameof(NavigationType.NavigationLinkCSS));
            var fieldLinkCSS = existingFieldLinkCSS ?? new FormFieldInfo();
            fieldLinkCSS.Name = nameof(NavigationType.NavigationLinkCSS);
            fieldLinkCSS.AllowEmpty = true;
            fieldLinkCSS.Precision = 0;
            fieldLinkCSS.Size = 100;
            fieldLinkCSS.DataType = "text";
            fieldLinkCSS.Enabled = true;
            fieldLinkCSS.Visible = true;
            fieldLinkCSS.Guid = Guid.Parse("f8a4d722-7fc4-4c78-819f-ee9c6db32dd9");
            fieldLinkCSS.SetComponentName("Kentico.Administration.TextInput");
            fieldLinkCSS.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, "Link CSS Class");
            fieldLinkCSS.SetPropertyValue(FormFieldPropertyEnum.FieldDescription, "Optional CSS Class applied to the navigation item.");
            fieldLinkCSS.SetPropertyValue(FormFieldPropertyEnum.FieldDescriptionAsHtml, "False");
            fieldLinkCSS.SetPropertyValue(FormFieldPropertyEnum.ExplanationTextAsHtml, "False");

            if (existingFieldLinkCSS != null) {
                contentItemCommonDataForm.UpdateFormField(nameof(NavigationType.NavigationLinkCSS), fieldLinkCSS);
            } else {
                contentItemCommonDataForm.AddFormItem(fieldLinkCSS);
            }

            // Add or Update IsDynamic Field
            var existingFieldIsDynamic = contentItemCommonDataForm.GetFormField(nameof(NavigationType.IsDynamic));
            var fieldIsDynamic = existingFieldIsDynamic ?? new FormFieldInfo();
            fieldIsDynamic.Name = nameof(NavigationType.IsDynamic);
            fieldIsDynamic.AllowEmpty = false;
            fieldIsDynamic.Precision = 0;
            fieldIsDynamic.DataType = "boolean";
            fieldIsDynamic.Enabled = true;
            fieldIsDynamic.Visible = true;
            fieldIsDynamic.Guid = Guid.Parse("7511f4b1-a743-4888-b7b1-dcde8b846191");
            fieldIsDynamic.SetComponentName("Kentico.Administration.Checkbox");
            fieldIsDynamic.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, "Is Dynamic");
            fieldIsDynamic.SetPropertyValue(FormFieldPropertyEnum.FieldDescription, "If checked, allows you to define child items dynamically");
            fieldIsDynamic.SetPropertyValue(FormFieldPropertyEnum.FieldDescriptionAsHtml, "False");
            fieldIsDynamic.SetPropertyValue(FormFieldPropertyEnum.ExplanationTextAsHtml, "False");

            if (existingFieldIsDynamic != null) {
                contentItemCommonDataForm.UpdateFormField(nameof(NavigationType.IsDynamic), fieldIsDynamic);
            } else {
                contentItemCommonDataForm.AddFormItem(fieldIsDynamic);
            }

            // Add or Update DynamicCodeName Field
            var existingFieldDynamicCodeName = contentItemCommonDataForm.GetFormField(nameof(NavigationType.DynamicCodeName));
            var fieldDynamicCodeName = existingFieldDynamicCodeName ?? new FormFieldInfo();
            fieldDynamicCodeName.Name = nameof(NavigationType.DynamicCodeName);
            fieldDynamicCodeName.AllowEmpty = true;
            fieldDynamicCodeName.Precision = 0;
            fieldDynamicCodeName.Size = 100;
            fieldDynamicCodeName.DataType = "text";
            fieldDynamicCodeName.Enabled = true;
            fieldDynamicCodeName.Visible = true;
            fieldDynamicCodeName.Guid = Guid.Parse("623cb11c-22ef-497e-984c-b6fd984624f3");
            fieldDynamicCodeName.SetComponentName("Kentico.Administration.TextInput");
            fieldDynamicCodeName.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, "Dynamic Menu Identifier");
            fieldDynamicCodeName.SetPropertyValue(FormFieldPropertyEnum.FieldDescription, "This will be passed to IDynamicMenuGenerator for you to return the items.");
            fieldDynamicCodeName.SetPropertyValue(FormFieldPropertyEnum.FieldDescriptionAsHtml, "False");
            fieldDynamicCodeName.SetPropertyValue(FormFieldPropertyEnum.ExplanationTextAsHtml, "False");
            fieldDynamicCodeName.VisibilityConditionConfigurationXmlData = "<VisibilityConditionConfiguration><Identifier>Kentico.Administration.IsTrueVisibilityCondition</Identifier><Properties><PropertyName>IsDynamic</PropertyName></Properties></VisibilityConditionConfiguration>";

            if (existingFieldDynamicCodeName != null) {
                contentItemCommonDataForm.UpdateFormField(nameof(NavigationType.DynamicCodeName), fieldDynamicCodeName);
            } else {
                contentItemCommonDataForm.AddFormItem(fieldDynamicCodeName);
            }

            // Add the schema
            if (formInfo.GetFormSchema(memberPermissionSchema.Guid.ToString().ToLower()) == null) {
                var baseMetaDataSchemaReference = new FormSchemaInfo() {
                    Guid = memberPermissionSchema.Guid,
                    Name = memberPermissionSchema.Guid.ToString()
                };
                formInfo.ItemsList.Add(baseMetaDataSchemaReference);
            }

            classNavigation.ClassFormDefinition = formInfo.GetXmlDefinition();
            if (classNavigation.HasChanged) {
                DataClassInfoProvider.SetDataClassInfo(classNavigation);
            }

        }

    }
}

