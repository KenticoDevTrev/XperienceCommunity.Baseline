using CMS.ContentEngine.Internal;
using CMS.Core;
using CMS.FormEngine;
using CMS.Membership;
using Generic;

namespace Core.Installers
{
    public class BaselineModuleInstaller(BaselineCoreInstallerOptions baselineCoreInstallerOptions)
    {
        private readonly BaselineCoreInstallerOptions _baselineCoreInstallerOptions = baselineCoreInstallerOptions;

        private const string _hasImageSchemaGuid = "03dda2f6-b776-48b2-92d7-c110682febe0";

        public bool InstallationRan { get; set; } = false;

        public Task Install()
        {
            if (_baselineCoreInstallerOptions.AddMemberFields) {
                AddMemberFields();
            }

            CreateMetadataReusableSchema();
            CreateRedirectReusableSchema();
            CreateHasImageReusableSchema();

            InstallationRan = true;

            return Task.CompletedTask;
        }

        private static void CreateHasImageReusableSchema()
        {
            var contentItemCommonDataForm = FormHelper.GetFormInfo(ContentItemCommonDataInfo.OBJECT_TYPE, false);

            // Add Image Schema
            var imageSchemaGuid = Guid.Parse(_hasImageSchemaGuid);
            var imageSchema = contentItemCommonDataForm.GetFormSchema("Generic.HasImage");
            if (imageSchema == null) {
                contentItemCommonDataForm.ItemsList.Add(new FormSchemaInfo() {
                    Name = "Generic.HasImage",
                    Description = @"Place this Reusable Schema on the content type you wish to be able to select from for the Base.Metadata Resuable Schema's OG Image.

Should configure in the Core Baseline ContentItemAssetOptionsConfiguration.",
                    Caption = "Has Image",
                    Guid = imageSchemaGuid
                });
            }
        }
        private static void CreateMetadataReusableSchema()
        {
            var contentItemCommonData = DataClassInfoProvider.GetClasses().WhereEquals(nameof(DataClassInfo.ClassName), ContentItemCommonDataInfo.OBJECT_TYPE).FirstOrDefault() ?? throw new Exception("No Content Item Common Data Class Found, you got bigger problems than installing Member Roles!");
            var contentItemCommonDataForm = FormHelper.GetFormInfo(ContentItemCommonDataInfo.OBJECT_TYPE, false);
            var schemaGuid = Guid.Parse("cbd68689-f238-4a7e-9437-ba5182822c70");

            // Add Schema
            var schema = contentItemCommonDataForm.GetFormSchema("Base.Metadata");
            if (schema == null) {
                contentItemCommonDataForm.ItemsList.Add(new FormSchemaInfo() {
                    Name = "Base.Metadata",
                    Description = @"Contains Meta Data for your page. In KX13 Baseline, was part of the
Generic.BaseInheritedPage",
                    Caption = "Page Metadata",
                    Guid = schemaGuid
                });
            }

            // Add or Update MenuName Field
            var existingFieldMenuName = contentItemCommonDataForm.GetFormField(nameof(IBaseMetadata.MetaData_PageName));
            var fieldMenuName = existingFieldMenuName ?? new FormFieldInfo();
            fieldMenuName.Name = nameof(IBaseMetadata.MetaData_PageName);
            fieldMenuName.AllowEmpty = true;
            fieldMenuName.Precision = 0;
            fieldMenuName.DataType = "text";
            fieldMenuName.Enabled = true;
            fieldMenuName.Visible = true;
            fieldMenuName.Size = 200;
            fieldMenuName.Guid = Guid.Parse("70dfb72a-d2a2-49df-84dc-61c0068163f9");
            fieldMenuName.SetComponentName("Kentico.Administration.TextInput");
            fieldMenuName.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, "Page Name");
            fieldMenuName.SetPropertyValue(FormFieldPropertyEnum.FieldDescription, "What gets displayed on Menus, Breadcrumbs, etc.");
            fieldMenuName.SetPropertyValue(FormFieldPropertyEnum.FieldDescriptionAsHtml, "False");
            fieldMenuName.Properties["kxp_schema_identifier"] = schemaGuid.ToString().ToLower();

            if (existingFieldMenuName != null) {
                contentItemCommonDataForm.UpdateFormField(nameof(IBaseMetadata.MetaData_PageName), fieldMenuName);
            } else {
                contentItemCommonDataForm.AddFormItem(fieldMenuName);
            }

            // Add or Update Title Field
            var existingTitleField = contentItemCommonDataForm.GetFormField(nameof(IBaseMetadata.MetaData_Title));
            var titleField = existingTitleField ?? new FormFieldInfo();
            titleField.Name = nameof(IBaseMetadata.MetaData_Title);
            titleField.AllowEmpty = true;
            titleField.Precision = 0;
            titleField.DataType = "longtext";
            titleField.Enabled = true;
            titleField.Visible = true;
            titleField.Guid = Guid.Parse("1e716d33-9f3c-42b4-aa99-330f44976058");
            titleField.SetComponentName("Kentico.Administration.TextInput");
            titleField.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, "Page Title");
            titleField.SetPropertyValue(FormFieldPropertyEnum.FieldDescription, "If empty, will default to the Page Name");
            titleField.SetPropertyValue(FormFieldPropertyEnum.FieldDescriptionAsHtml, "False");
            titleField.Properties["kxp_schema_identifier"] = schemaGuid.ToString().ToLower();

            if (existingTitleField != null) {
                contentItemCommonDataForm.UpdateFormField(nameof(IBaseMetadata.MetaData_Title), titleField);
            } else {
                contentItemCommonDataForm.AddFormItem(titleField);
            }

            // Add or Update Description Field
            var existingDescriptionField = contentItemCommonDataForm.GetFormField(nameof(IBaseMetadata.MetaData_Description));
            var descriptionField = existingDescriptionField ?? new FormFieldInfo();
            descriptionField.Name = nameof(IBaseMetadata.MetaData_Description);
            descriptionField.AllowEmpty = true;
            descriptionField.Precision = 0;
            descriptionField.DataType = "longtext";
            descriptionField.Enabled = true;
            descriptionField.Visible = true;
            descriptionField.Guid = Guid.Parse("3788a858-7695-4837-9d1e-164d39eb0d06");
            descriptionField.SetComponentName("Kentico.Administration.TextArea");
            descriptionField.Settings["CopyButtonVisible"] = "False";
            descriptionField.Settings["MaxRowsNumber"] = "5";
            descriptionField.Settings["MinRowsNumber"] = "3";

            descriptionField.SetPropertyValue(FormFieldPropertyEnum.ExplanationTextAsHtml, "False");
            descriptionField.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, "Description");
            descriptionField.SetPropertyValue(FormFieldPropertyEnum.FieldDescriptionAsHtml, "False");
            descriptionField.Properties["kxp_schema_identifier"] = schemaGuid.ToString().ToLower();

            if (existingDescriptionField != null) {
                contentItemCommonDataForm.UpdateFormField(nameof(IBaseMetadata.MetaData_Description), descriptionField);
            } else {
                contentItemCommonDataForm.AddFormItem(descriptionField);
            }

            // Add or Update Keywords Field
            var existingKeywordsField = contentItemCommonDataForm.GetFormField(nameof(IBaseMetadata.MetaData_Keywords));
            var keywordsField = existingKeywordsField ?? new FormFieldInfo();
            keywordsField.Name = nameof(IBaseMetadata.MetaData_Keywords);
            keywordsField.AllowEmpty = true;
            keywordsField.Precision = 0;
            keywordsField.DataType = "longtext";
            keywordsField.Enabled = true;
            keywordsField.Visible = true;
            keywordsField.Guid = Guid.Parse("e71ff512-61a8-4e85-9892-2d7ecba7f0ff");
            keywordsField.SetComponentName("Kentico.Administration.TextInput");
            keywordsField.SetPropertyValue(FormFieldPropertyEnum.ExplanationTextAsHtml, "False");
            keywordsField.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, "Keywords");
            keywordsField.SetPropertyValue(FormFieldPropertyEnum.FieldDescriptionAsHtml, "False");
            keywordsField.Properties["kxp_schema_identifier"] = schemaGuid.ToString().ToLower();

            if (existingKeywordsField != null) {
                contentItemCommonDataForm.UpdateFormField(nameof(IBaseMetadata.MetaData_Keywords), keywordsField);
            } else {
                contentItemCommonDataForm.AddFormItem(keywordsField);
            }

            // Add or Update OG Image Field
            var existingOGImageField = contentItemCommonDataForm.GetFormField(nameof(IBaseMetadata.MetaData_OGImage));
            var ogImageField = existingOGImageField ?? new FormFieldInfo();
            ogImageField.Name = nameof(IBaseMetadata.MetaData_OGImage);
            ogImageField.AllowEmpty = true;
            ogImageField.Precision = 0;
            ogImageField.DataType = "contentitemreference";
            ogImageField.Enabled = true;
            ogImageField.Visible = true;
            ogImageField.Guid = Guid.Parse("03eb66ce-235e-48ac-a488-73b08d222ff1");
            ogImageField.SetComponentName("Kentico.Administration.ContentItemSelector");
            ogImageField.Settings["MaximumAssets"] = "1";
            ogImageField.Settings["MinimumItems"] = "0";
            ogImageField.Settings["SelectionType"] = "reusableFieldSchemas";
            ogImageField.Settings["AllowedSchemaIdentifiers"] = $"[\"{_hasImageSchemaGuid}\"]";

            ogImageField.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, "OG Image");
            ogImageField.SetPropertyValue(FormFieldPropertyEnum.FieldDescription, "For Social Media Sharing only.  Should be 1280×720, JPEG PNG or WebP.");
            ogImageField.SetPropertyValue(FormFieldPropertyEnum.ExplanationTextAsHtml, "False");
            ogImageField.SetPropertyValue(FormFieldPropertyEnum.FieldDescriptionAsHtml, "False");
            ogImageField.Properties["kxp_schema_identifier"] = schemaGuid.ToString().ToLower();

            if (existingOGImageField != null) {
                contentItemCommonDataForm.UpdateFormField(nameof(IBaseMetadata.MetaData_OGImage), ogImageField);
            } else {
                contentItemCommonDataForm.AddFormItem(ogImageField);
            }

            // Add or Update NoIndex Field
            var existingFieldNoIndex = contentItemCommonDataForm.GetFormField(nameof(IBaseMetadata.MetaData_NoIndex));
            var fieldNoIndex = existingFieldNoIndex ?? new FormFieldInfo();
            fieldNoIndex.Name = nameof(IBaseMetadata.MetaData_NoIndex);
            fieldNoIndex.AllowEmpty = true;
            fieldNoIndex.Precision = 0;
            fieldNoIndex.DataType = "boolean";
            fieldNoIndex.Enabled = true;
            fieldNoIndex.Visible = true;
            fieldNoIndex.Guid = Guid.Parse("dfd21f8a-7c4d-4420-bedc-40c3163f151f");
            fieldNoIndex.SetComponentName("Kentico.Administration.Checkbox");
            fieldNoIndex.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, "No Index");
            fieldNoIndex.SetPropertyValue(FormFieldPropertyEnum.FieldDescription, "Indicates that this page should not be indexed by search engines.");
            fieldNoIndex.SetPropertyValue(FormFieldPropertyEnum.ExplanationTextAsHtml, "False");
            fieldNoIndex.SetPropertyValue(FormFieldPropertyEnum.FieldDescriptionAsHtml, "False");
            fieldNoIndex.Properties["kxp_schema_identifier"] = schemaGuid.ToString().ToLower();

            if (existingFieldNoIndex != null) {
                contentItemCommonDataForm.UpdateFormField(nameof(IBaseMetadata.MetaData_NoIndex), fieldNoIndex);
            } else {
                contentItemCommonDataForm.AddFormItem(fieldNoIndex);
            }


            contentItemCommonData.ClassFormDefinition = contentItemCommonDataForm.GetXmlDefinition();

            if (contentItemCommonData.HasChanged) {
                DataClassInfoProvider.SetDataClassInfo(contentItemCommonData);
            }
        }

        private static void CreateRedirectReusableSchema()
        {
            var contentItemCommonData = DataClassInfoProvider.GetClasses().WhereEquals(nameof(DataClassInfo.ClassName), ContentItemCommonDataInfo.OBJECT_TYPE).FirstOrDefault() ?? throw new Exception("No Content Item Common Data Class Found, you got bigger problems than installing Member Roles!");
            var contentItemCommonDataForm = FormHelper.GetFormInfo(ContentItemCommonDataInfo.OBJECT_TYPE, false);
            var schemaGuid = Guid.Parse("95e3dcfc-b550-4000-b7bb-02cdd81b8161");

            // Add Schema
            var schema = contentItemCommonDataForm.GetFormSchema("Base.Redirect");
            if (schema == null) {
                contentItemCommonDataForm.ItemsList.Add(new FormSchemaInfo() {
                    Name = "Base.Redirect",
                    Description = @"Allows for redirection logic. In KX13 Baseline it was part of the
Generic.BaseInheritedPage and Generic.RedirectOnlyInheritedPage",
                    Caption = "Redirect",
                    Guid = schemaGuid
                });
            }

            // Add or Update PageRedirection Field
            var existingPageRedirectionField = contentItemCommonDataForm.GetFormField(nameof(IBaseRedirect.PageRedirectionType));
            var pageRedirectionField = existingPageRedirectionField ?? new FormFieldInfo();
            pageRedirectionField.Name = nameof(IBaseRedirect.PageRedirectionType);
            pageRedirectionField.AllowEmpty = true;
            pageRedirectionField.Precision = 0;
            pageRedirectionField.Size = 200;
            pageRedirectionField.DataType = "text";
            pageRedirectionField.Enabled = true;
            pageRedirectionField.Visible = true;
            pageRedirectionField.Guid = Guid.Parse("d72506d4-f799-40e0-8d17-62bd3ae49c00");
            pageRedirectionField.SetComponentName("Kentico.Administration.DropDownSelector");
            pageRedirectionField.Settings["Options"] = "None\nInternal\nExternal\nFirstChild;First Child";
            pageRedirectionField.Settings["OptionsValueSeparator"] = ";";
            pageRedirectionField.Settings["Placeholder"] = "None";

            pageRedirectionField.SetPropertyValue(FormFieldPropertyEnum.DefaultValue, "None");
            pageRedirectionField.SetPropertyValue(FormFieldPropertyEnum.ExplanationTextAsHtml, "False");
            pageRedirectionField.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, "Redirection Type");
            pageRedirectionField.SetPropertyValue(FormFieldPropertyEnum.FieldDescriptionAsHtml, "False");
            pageRedirectionField.Properties["kxp_schema_identifier"] = schemaGuid.ToString().ToLower();

            if (existingPageRedirectionField != null) {
                contentItemCommonDataForm.UpdateFormField(nameof(IBaseRedirect.PageRedirectionType), pageRedirectionField);
            } else {
                contentItemCommonDataForm.AddFormItem(pageRedirectionField);
            }

            // Add or Update PageInternalRedirectionPage Field
            var fieldExistingPageInternalRedirectPage = contentItemCommonDataForm.GetFormField(nameof(IBaseRedirect.PageInternalRedirectPage));
            var fieldPageInternalRedirectPage = fieldExistingPageInternalRedirectPage ?? new FormFieldInfo();
            fieldPageInternalRedirectPage.Name = nameof(IBaseRedirect.PageInternalRedirectPage);
            fieldPageInternalRedirectPage.AllowEmpty = true;
            fieldPageInternalRedirectPage.Precision = 0;
            fieldPageInternalRedirectPage.DataType = "webpages";
            fieldPageInternalRedirectPage.Enabled = true;
            fieldPageInternalRedirectPage.Visible = true;
            fieldPageInternalRedirectPage.Guid = Guid.Parse("f47ba3f8-1625-4897-aabf-a701a639c23b");
            fieldPageInternalRedirectPage.SetComponentName("Kentico.Administration.WebPageSelector");
            fieldPageInternalRedirectPage.Settings["MaximumPages"] = "1";
            fieldPageInternalRedirectPage.Settings["Sortable"] = "False";

            fieldPageInternalRedirectPage.SetPropertyValue(FormFieldPropertyEnum.ExplanationTextAsHtml, "False");
            fieldPageInternalRedirectPage.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, "Internal URL");
            fieldPageInternalRedirectPage.SetPropertyValue(FormFieldPropertyEnum.FieldDescriptionAsHtml, "False");
            fieldPageInternalRedirectPage.Properties["kxp_schema_identifier"] = schemaGuid.ToString().ToLower();

            if (fieldExistingPageInternalRedirectPage != null) {
                contentItemCommonDataForm.UpdateFormField(nameof(IBaseRedirect.PageInternalRedirectPage), fieldPageInternalRedirectPage);
            } else {
                contentItemCommonDataForm.AddFormItem(fieldPageInternalRedirectPage);
            }

            // Add or Update PageExternalRedirectURL Field
            var fieldExistingPageExternalRedirectURL = contentItemCommonDataForm.GetFormField(nameof(IBaseRedirect.PageExternalRedirectURL));
            var fieldPageExternalRedirectURL = fieldExistingPageExternalRedirectURL ?? new FormFieldInfo();
            fieldPageExternalRedirectURL.Name = nameof(IBaseRedirect.PageExternalRedirectURL);
            fieldPageExternalRedirectURL.AllowEmpty = true;
            fieldPageExternalRedirectURL.Precision = 0;
            fieldPageExternalRedirectURL.Size = 512;
            fieldPageExternalRedirectURL.DataType = "text";
            fieldPageExternalRedirectURL.Enabled = true;
            fieldPageExternalRedirectURL.Visible = true;
            fieldPageExternalRedirectURL.Guid = Guid.Parse("3adfa20c-b5a3-4c4a-838e-54192743e35e");
            fieldPageExternalRedirectURL.SetComponentName("Kentico.Administration.Link");
            fieldPageExternalRedirectURL.Settings["OpenInNewTab"] = "True";

            fieldPageExternalRedirectURL.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, "External URL");
            fieldPageExternalRedirectURL.SetPropertyValue(FormFieldPropertyEnum.ExplanationTextAsHtml, "False");
            fieldPageExternalRedirectURL.SetPropertyValue(FormFieldPropertyEnum.FieldDescriptionAsHtml, "False");
            fieldPageExternalRedirectURL.Properties["kxp_schema_identifier"] = schemaGuid.ToString().ToLower();

            if (fieldExistingPageExternalRedirectURL != null) {
                contentItemCommonDataForm.UpdateFormField(nameof(IBaseRedirect.PageExternalRedirectURL), fieldPageExternalRedirectURL);
            } else {
                contentItemCommonDataForm.AddFormItem(fieldPageExternalRedirectURL);
            }

            // Add or Update PageFirstChildClassName Field
            var fieldExistingPageFirstChildClassName = contentItemCommonDataForm.GetFormField(nameof(IBaseRedirect.PageFirstChildClassName));
            var fieldPageFirstChildClassName = fieldExistingPageFirstChildClassName ?? new FormFieldInfo();
            fieldPageFirstChildClassName.Name = nameof(IBaseRedirect.PageFirstChildClassName);
            fieldPageFirstChildClassName.AllowEmpty = true;
            fieldPageFirstChildClassName.Precision = 0;
            fieldPageFirstChildClassName.Size = 200;
            fieldPageFirstChildClassName.DataType = "text";
            fieldPageFirstChildClassName.Enabled = true;
            fieldPageFirstChildClassName.Visible = true;
            fieldPageFirstChildClassName.Guid = Guid.Parse("79a85749-bedf-4f85-8f95-b024762fa6e3");
            fieldPageFirstChildClassName.SetComponentName("Kentico.Administration.TextInput");

            fieldPageFirstChildClassName.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, "First Child Page Type");
            fieldPageFirstChildClassName.SetPropertyValue(FormFieldPropertyEnum.ExplanationTextAsHtml, "False");
            fieldPageFirstChildClassName.SetPropertyValue(FormFieldPropertyEnum.FieldDescriptionAsHtml, "False");
            fieldPageFirstChildClassName.Properties["kxp_schema_identifier"] = schemaGuid.ToString().ToLower();

            if (fieldExistingPageFirstChildClassName != null) {
                contentItemCommonDataForm.UpdateFormField(nameof(IBaseRedirect.PageFirstChildClassName), fieldPageFirstChildClassName);
            } else {
                contentItemCommonDataForm.AddFormItem(fieldPageFirstChildClassName);
            }

            // Add or Update PageUsePermanentRedirects Field
            var fieldExistingPageUsePermanentRedirects = contentItemCommonDataForm.GetFormField(nameof(IBaseRedirect.PageUsePermanentRedirects));
            var fieldPageUsePermanentRedirects = fieldExistingPageUsePermanentRedirects ?? new FormFieldInfo();
            fieldPageUsePermanentRedirects.Name = nameof(IBaseRedirect.PageUsePermanentRedirects);
            fieldPageUsePermanentRedirects.AllowEmpty = true;
            fieldPageUsePermanentRedirects.Precision = 0;
            fieldPageUsePermanentRedirects.DataType = "boolean";
            fieldPageUsePermanentRedirects.Enabled = true;
            fieldPageUsePermanentRedirects.Visible = true;
            fieldPageUsePermanentRedirects.Guid = Guid.Parse("a808c854-d2ce-4fd0-9d32-d9a185f73d5f");
            fieldPageUsePermanentRedirects.SetComponentName("Kentico.Administration.Checkbox");

            fieldPageUsePermanentRedirects.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, "Use Permanent (301) Redirects?");
            fieldPageUsePermanentRedirects.SetPropertyValue(FormFieldPropertyEnum.ExplanationTextAsHtml, "False");
            fieldPageUsePermanentRedirects.SetPropertyValue(FormFieldPropertyEnum.FieldDescriptionAsHtml, "False");
            fieldPageUsePermanentRedirects.Properties["kxp_schema_identifier"] = schemaGuid.ToString().ToLower();

            if (fieldExistingPageUsePermanentRedirects != null) {
                contentItemCommonDataForm.UpdateFormField(nameof(IBaseRedirect.PageUsePermanentRedirects), fieldPageUsePermanentRedirects);
            } else {
                contentItemCommonDataForm.AddFormItem(fieldPageUsePermanentRedirects);
            }

            contentItemCommonData.ClassFormDefinition = contentItemCommonDataForm.GetXmlDefinition();

            if (contentItemCommonData.HasChanged) {
                DataClassInfoProvider.SetDataClassInfo(contentItemCommonData);
            }
        }

        private static void AddMemberFields()
        {

            // get Member Class
            var memberClass = DataClassInfoProvider.GetClasses()
                .WhereEquals(nameof(DataClassInfo.ClassName), MemberInfo.OBJECT_TYPE)
                .FirstOrDefault();

            if (memberClass != null) {
                // get class info
                var form = new FormInfo(memberClass.ClassFormDefinition);

                var existingFirstName = form.GetFormField(nameof(ApplicationUserBaseline.MemberFirstName));
                var firstNameField = existingFirstName ?? new FormFieldInfo();
                firstNameField.Name = nameof(ApplicationUserBaseline.MemberFirstName);
                firstNameField.Precision = 0;
                firstNameField.Size = 100;
                firstNameField.DataType = "text";
                firstNameField.Enabled = true;
                firstNameField.AllowEmpty = true;
                if (existingFirstName != null) {
                    form.UpdateFormField(nameof(ApplicationUserBaseline.MemberFirstName), firstNameField);
                } else {
                    form.AddFormItem(firstNameField);
                }

                var existingMiddleName = form.GetFormField(nameof(ApplicationUserBaseline.MemberMiddleName));
                var middleNameField = existingMiddleName ?? new FormFieldInfo();
                middleNameField.Name = nameof(ApplicationUserBaseline.MemberMiddleName);
                middleNameField.Precision = 0;
                middleNameField.Size = 100;
                middleNameField.DataType = "text";
                middleNameField.Enabled = true;
                middleNameField.AllowEmpty = true;
                if (existingMiddleName != null) {
                    form.UpdateFormField(nameof(ApplicationUserBaseline.MemberMiddleName), middleNameField);
                } else {
                    form.AddFormItem(middleNameField);
                }

                var existingLastName = form.GetFormField(nameof(ApplicationUserBaseline.MemberLastName));
                var lastNameField = existingLastName ?? new FormFieldInfo();
                lastNameField.Name = nameof(ApplicationUserBaseline.MemberLastName);
                lastNameField.Precision = 0;
                lastNameField.Size = 100;
                lastNameField.DataType = "text";
                lastNameField.Enabled = true;
                lastNameField.AllowEmpty = true;
                if (existingLastName != null) {
                    form.UpdateFormField(nameof(ApplicationUserBaseline.MemberLastName), lastNameField);
                } else {
                    form.AddFormItem(lastNameField);
                }

                memberClass.ClassFormDefinition = form.GetXmlDefinition();

                if (memberClass.HasChanged) {
                    DataClassInfoProvider.SetDataClassInfo(memberClass);
                }
            }
        }
    }
}
