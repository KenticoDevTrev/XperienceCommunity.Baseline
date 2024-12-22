using CMS.ContentEngine.Internal;
using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;
using CMS.FormEngine;
using Generic;
using Image = Generic.Image;
using File = Generic.File;

namespace Admin.Installer
{
    public class StartingSiteInstaller(
        StartingSiteInstallationOptions startingSiteInstallationOptions,
        IEventLogService eventLogService,
        IInfoProvider<ContentTypeChannelInfo> contentTypeChannelInfoProvider,
        IInfoProvider<ChannelInfo> channelInfoProvider,
        IInfoProvider<SettingsKeyInfo> settingsKeyInfoProvider)
    {
        private readonly StartingSiteInstallationOptions _startingSiteInstallationOptions = startingSiteInstallationOptions;
        private readonly IEventLogService _eventLogService = eventLogService;
        private readonly IInfoProvider<ContentTypeChannelInfo> _contentTypeChannelInfoProvider = contentTypeChannelInfoProvider;
        private readonly IInfoProvider<ChannelInfo> _channelInfoProvider = channelInfoProvider;
        private readonly IInfoProvider<SettingsKeyInfo> _settingsKeyInfoProvider = settingsKeyInfoProvider;

        public bool InstallationRan { get; set; } = false;

        public Task Install()
        {
            if (_startingSiteInstallationOptions.AddHomePageType) {
                CreateHomeWebpage();
            }
            if (_startingSiteInstallationOptions.AddBasicPageType) {
                CreateBasicWebpage();
            }
            var mediaTypeSet = false;
            if (_startingSiteInstallationOptions.AddImageContentType) {
                CreateImageContentType();
                mediaTypeSet = true;
            }
            if (_startingSiteInstallationOptions.AddFileContentType) {
                CreateFileContentType();
                mediaTypeSet = true;
            }
            if (_startingSiteInstallationOptions.AddAudioContentType) {
                CreateAudioContentType();
                mediaTypeSet = true;
            }
            if (_startingSiteInstallationOptions.AddVideoContentType) {
                CreateVideoContentType();
                mediaTypeSet = true;
            }
            if (mediaTypeSet) {
                // Ensure Media types are allowed in assets
                EnsureMediaTypesAllowed();
            }

            InstallationRan = true;

            return Task.CompletedTask;
        }

        private void EnsureMediaTypesAllowed()
        {
            var key = _settingsKeyInfoProvider.Get("CMSMediaFileAllowedExtensions");
            if (key != null) {
                var allowedExtensions = key.KeyValue?.ToLower().Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries) ?? [];
                var mediaExtensions = _startingSiteInstallationOptions.ImageFormatsSupported.ToLower().Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Union(
                     _startingSiteInstallationOptions.VideoFormatsSupported.ToLower().Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)).Union(
                     _startingSiteInstallationOptions.AudioFormatsSupported.ToLower().Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)).Union(
                     _startingSiteInstallationOptions.NonMediaFileFormatsSupported.ToLower().Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));

                var itemsMissing = mediaExtensions.Except(allowedExtensions);
                if (itemsMissing.Any()) {
                    key.KeyValue = string.Join(";", allowedExtensions.Union(itemsMissing).Distinct());
                    _settingsKeyInfoProvider.Set(key);
                }
            }
        }

        private void CreateImageContentType()
        {
            var contentItemCommonDataForm = FormHelper.GetFormInfo(ContentItemCommonDataInfo.OBJECT_TYPE, false);
            var imageSchema = contentItemCommonDataForm.GetFormSchema("Generic.HasImage");
           
            if (imageSchema == null) {
                _eventLogService.LogError("StartingSiteInstaller", "MissingSchemas", eventDescription: $"Can't create Image page type because one of the following schemas are missing: {(imageSchema == null ? "Base.Metadata" : "")}");
                return;
            }

            var existingClassImage = DataClassInfoProvider.GetClasses().WhereEquals(nameof(DataClassInfo.ClassName), "Generic.Image")
                .GetEnumerableTypedResult().FirstOrDefault();
            var classImage = existingClassImage ?? DataClassInfo.New("Generic.Image");
            classImage.ClassDisplayName = "Media File - Image";
            classImage.ClassName = "Generic.Image";
            classImage.ClassTableName = "Generic_Image";
            classImage.ClassGUID = Guid.Parse("20071D4B-400C-4D91-945D-D05A200A67B2");
            classImage.ClassIconClass = "xp-picture";
            classImage.ClassHasUnmanagedDbSchema = false;
            classImage.ClassType = "Content";
            classImage.ClassContentTypeType = "Reusable";
            classImage.ClassWebPageHasUrl = true;
            classImage.ClassShortName = "GenericImage";

            var formInfo = existingClassImage != null ? new FormInfo(existingClassImage.ClassFormDefinition) : FormHelper.GetBasicFormDefinition("ContentItemDataID");

            // The main field will always exist, but check Guid value to make sure stays identical.
            var existingFieldContentItemDataID = formInfo.GetFormField("ContentItemDataID");
            var guidMatches = existingFieldContentItemDataID.Guid.Equals(Guid.Parse("42383b40-69aa-4b31-a98e-0bba8d2d1e83"));
            if (!guidMatches) {
                existingFieldContentItemDataID.Guid = Guid.Parse("42383b40-69aa-4b31-a98e-0bba8d2d1e83");
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
            fieldContentItemDataCommonDataID.Guid = Guid.Parse("96dbd301-7dd9-4125-aed0-2a860a012bc2");

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
            fieldContentItemDataGUID.Guid = Guid.Parse("4c341d79-baea-4edd-9ff9-652e6ff72e3c");

            if (existingFieldContentItemDataGUID != null) {
                formInfo.UpdateFormField("ContentItemDataGUID", fieldContentItemDataGUID);
            } else {
                formInfo.AddFormItem(fieldContentItemDataGUID);
            }


            // Add or Update Title Field
            var existingFieldTitle = contentItemCommonDataForm.GetFormField(nameof(Image.ImageTitle));
            var fieldTitle = existingFieldTitle ?? new FormFieldInfo();
            fieldTitle.Name = nameof(Image.ImageTitle);
            fieldTitle.AllowEmpty = false;
            fieldTitle.Precision = 0;
            fieldTitle.Size = 100;
            fieldTitle.DataType = "text";
            fieldTitle.Enabled = true;
            fieldTitle.Visible = true;
            fieldTitle.Guid = Guid.Parse("2755f9e4-361c-43c6-9e7f-db2977aeb2f0");
            fieldTitle.SetComponentName("Kentico.Administration.TextInput");
            fieldTitle.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, "Title");
            fieldTitle.SetPropertyValue(FormFieldPropertyEnum.FieldDescriptionAsHtml, "False");
            fieldTitle.SetPropertyValue(FormFieldPropertyEnum.ExplanationTextAsHtml, "False");

            if (existingFieldTitle != null) {
                contentItemCommonDataForm.UpdateFormField(nameof(Image.ImageTitle), fieldTitle);
            } else {
                contentItemCommonDataForm.AddFormItem(fieldTitle);
            }

            // Add or Update Description Field
            var existingFieldDescription = contentItemCommonDataForm.GetFormField(nameof(Image.ImageDescription));
            var fieldDescription = existingFieldDescription ?? new FormFieldInfo();
            fieldDescription.Name = nameof(Image.ImageDescription);
            fieldDescription.AllowEmpty = true;
            fieldDescription.Precision = 0;
            fieldDescription.DataType = "longtext";
            fieldDescription.Enabled = true;
            fieldDescription.Visible = true;
            fieldDescription.Guid = Guid.Parse("8b929375-e276-40a1-ae1a-d245da5178fa");
            fieldDescription.SetComponentName("Kentico.Administration.TextInput");
            fieldDescription.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, "Description");
            fieldDescription.SetPropertyValue(FormFieldPropertyEnum.FieldDescription, "Used as the Alt if provided");
            fieldDescription.SetPropertyValue(FormFieldPropertyEnum.FieldDescriptionAsHtml, "False");
            fieldDescription.SetPropertyValue(FormFieldPropertyEnum.ExplanationTextAsHtml, "False");

            if (existingFieldDescription != null) {
                contentItemCommonDataForm.UpdateFormField(nameof(Image.ImageDescription), fieldDescription);
            } else {
                contentItemCommonDataForm.AddFormItem(fieldDescription);
            }

            // Add or Update File Field
            var existingFieldFile = contentItemCommonDataForm.GetFormField(nameof(Image.ImageFile));
            var fieldFile = existingFieldFile ?? new FormFieldInfo();
            fieldFile.Name = nameof(Image.ImageFile);
            fieldFile.AllowEmpty = true;
            fieldFile.Precision = 0;
            fieldFile.DataType = "contentitemasset";
            fieldFile.Enabled = true;
            fieldFile.Visible = true;
            fieldFile.Guid = Guid.Parse("a28a7e27-4593-40cf-8266-2ae4101723f5");
            fieldFile.SetComponentName("Kentico.Administration.ContentItemAssetUploader");
            fieldFile.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, "File");
            fieldFile.SetPropertyValue(FormFieldPropertyEnum.FieldDescriptionAsHtml, "False");
            fieldFile.SetPropertyValue(FormFieldPropertyEnum.ExplanationTextAsHtml, "False");
            fieldFile.Settings.Add("AllowedExtensions", _startingSiteInstallationOptions.ImageFormatsSupported);
            fieldFile.ValidationRuleConfigurationsXmlData = @"<validationrulesdata><ValidationRuleConfiguration><ValidationRuleIdentifier>Kentico.Administration.RequiredValue</ValidationRuleIdentifier><RuleValues><ErrorMessage>Must add a file</ErrorMessage></RuleValues></ValidationRuleConfiguration></validationrulesdata>";

            if (existingFieldFile != null) {
                contentItemCommonDataForm.UpdateFormField(nameof(Image.ImageFile), fieldFile);
            } else {
                contentItemCommonDataForm.AddFormItem(fieldFile);
            }

            // Add the schema
            if (formInfo.GetFormSchema(imageSchema.Guid.ToString().ToLower()) == null) {
                var baseMetaDataSchemaReference = new FormSchemaInfo() {
                    Guid = imageSchema.Guid,
                    Name = imageSchema.Guid.ToString()
                };
                formInfo.ItemsList.Add(baseMetaDataSchemaReference);
            }

            classImage.ClassFormDefinition = formInfo.GetXmlDefinition();
            if (classImage.HasChanged) {
                DataClassInfoProvider.SetDataClassInfo(classImage);
            }
        }

        private void CreateFileContentType()
        {
            var contentItemCommonDataForm = FormHelper.GetFormInfo(ContentItemCommonDataInfo.OBJECT_TYPE, false);

            var existingClassFile = DataClassInfoProvider.GetClasses().WhereEquals(nameof(DataClassInfo.ClassName), "Generic.File")
                .GetEnumerableTypedResult().FirstOrDefault();
            var classFile = existingClassFile ?? DataClassInfo.New("Generic.File");
            classFile.ClassDisplayName = "Media File - File";
            classFile.ClassName = "Generic.File";
            classFile.ClassTableName = "Generic_File";
            classFile.ClassGUID = Guid.Parse("5E65BAC3-1C45-43C9-8274-03ECB590115C");
            classFile.ClassIconClass = "xp-file";
            classFile.ClassHasUnmanagedDbSchema = false;
            classFile.ClassType = "Content";
            classFile.ClassContentTypeType = "Reusable";
            classFile.ClassWebPageHasUrl = true;
            classFile.ClassShortName = "GenericFile";

            var formInfo = existingClassFile != null ? new FormInfo(existingClassFile.ClassFormDefinition) : FormHelper.GetBasicFormDefinition("ContentItemDataID");

            // The main field will always exist, but check Guid value to make sure stays identical.
            var existingFieldContentItemDataID = formInfo.GetFormField("ContentItemDataID");
            var guidMatches = existingFieldContentItemDataID.Guid.Equals(Guid.Parse("3f423239-9f47-4e36-a9f3-e4c56161429d"));
            if (!guidMatches) {
                existingFieldContentItemDataID.Guid = Guid.Parse("3f423239-9f47-4e36-a9f3-e4c56161429d");
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
            fieldContentItemDataCommonDataID.Guid = Guid.Parse("ad06c4cc-240b-4203-bafa-661cae1e5233");

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
            fieldContentItemDataGUID.Guid = Guid.Parse("af147653-72d9-4512-ac7e-657aa7abe984");

            if (existingFieldContentItemDataGUID != null) {
                formInfo.UpdateFormField("ContentItemDataGUID", fieldContentItemDataGUID);
            } else {
                formInfo.AddFormItem(fieldContentItemDataGUID);
            }


            // Add or Update Title Field
            var existingFieldTitle = contentItemCommonDataForm.GetFormField(nameof(File.FileTitle));
            var fieldTitle = existingFieldTitle ?? new FormFieldInfo();
            fieldTitle.Name = nameof(File.FileTitle);
            fieldTitle.AllowEmpty = false;
            fieldTitle.Precision = 0;
            fieldTitle.Size = 100;
            fieldTitle.DataType = "text";
            fieldTitle.Enabled = true;
            fieldTitle.Visible = true;
            fieldTitle.Guid = Guid.Parse("e857615c-0bd1-4115-8f0b-0baaf43c8a34");
            fieldTitle.SetComponentName("Kentico.Administration.TextInput");
            fieldTitle.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, "Title");
            fieldTitle.SetPropertyValue(FormFieldPropertyEnum.FieldDescriptionAsHtml, "False");
            fieldTitle.SetPropertyValue(FormFieldPropertyEnum.ExplanationTextAsHtml, "False");

            if (existingFieldTitle != null) {
                contentItemCommonDataForm.UpdateFormField(nameof(File.FileTitle), fieldTitle);
            } else {
                contentItemCommonDataForm.AddFormItem(fieldTitle);
            }

            // Add or Update Description Field
            var existingFieldDescription = contentItemCommonDataForm.GetFormField(nameof(File.FileDescription));
            var fieldDescription = existingFieldDescription ?? new FormFieldInfo();
            fieldDescription.Name = nameof(File.FileDescription);
            fieldDescription.AllowEmpty = true;
            fieldDescription.Precision = 0;
            fieldDescription.DataType = "longtext";
            fieldDescription.Enabled = true;
            fieldDescription.Visible = true;
            fieldDescription.Guid = Guid.Parse("b8f1065c-4fcc-4374-99f3-0765de13ca1f");
            fieldDescription.SetComponentName("Kentico.Administration.TextInput");
            fieldDescription.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, "Description");
            fieldDescription.SetPropertyValue(FormFieldPropertyEnum.FieldDescriptionAsHtml, "False");
            fieldDescription.SetPropertyValue(FormFieldPropertyEnum.ExplanationTextAsHtml, "False");

            if (existingFieldDescription != null) {
                contentItemCommonDataForm.UpdateFormField(nameof(File.FileDescription), fieldDescription);
            } else {
                contentItemCommonDataForm.AddFormItem(fieldDescription);
            }

            // Add or Update File Field
            var existingFieldFile = contentItemCommonDataForm.GetFormField(nameof(File.FileFile));
            var fieldFile = existingFieldFile ?? new FormFieldInfo();
            fieldFile.Name = nameof(File.FileFile);
            fieldFile.AllowEmpty = true;
            fieldFile.Precision = 0;
            fieldFile.DataType = "contentitemasset";
            fieldFile.Enabled = true;
            fieldFile.Visible = true;
            fieldFile.Guid = Guid.Parse("28b7d7fc-e090-44c4-aab0-462553bb95fa");
            fieldFile.SetComponentName("Kentico.Administration.ContentItemAssetUploader");
            fieldFile.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, "File");
            fieldFile.SetPropertyValue(FormFieldPropertyEnum.FieldDescriptionAsHtml, "False");
            fieldFile.SetPropertyValue(FormFieldPropertyEnum.ExplanationTextAsHtml, "False");
            fieldFile.Settings.Add("AllowedExtensions", _startingSiteInstallationOptions.NonMediaFileFormatsSupported);
            fieldFile.ValidationRuleConfigurationsXmlData = @"<validationrulesdata><ValidationRuleConfiguration><ValidationRuleIdentifier>Kentico.Administration.RequiredValue</ValidationRuleIdentifier><RuleValues><ErrorMessage>Must add a file</ErrorMessage></RuleValues></ValidationRuleConfiguration></validationrulesdata>";

            if (existingFieldFile != null) {
                contentItemCommonDataForm.UpdateFormField(nameof(File.FileFile), fieldFile);
            } else {
                contentItemCommonDataForm.AddFormItem(fieldFile);
            }

            classFile.ClassFormDefinition = formInfo.GetXmlDefinition();
            if (classFile.HasChanged) {
                DataClassInfoProvider.SetDataClassInfo(classFile);
            }
        }

        private void CreateAudioContentType()
        {
            var contentItemCommonDataForm = FormHelper.GetFormInfo(ContentItemCommonDataInfo.OBJECT_TYPE, false);

            var existingClassAudio = DataClassInfoProvider.GetClasses().WhereEquals(nameof(DataClassInfo.ClassName), "Generic.Audio")
                .GetEnumerableTypedResult().FirstOrDefault();
            var classAudio = existingClassAudio ?? DataClassInfo.New("Generic.Audio");
            classAudio.ClassDisplayName = "Media File - Audio";
            classAudio.ClassName = "Generic.Audio";
            classAudio.ClassTableName = "Generic_Audio";
            classAudio.ClassGUID = Guid.Parse("A9E1EE4F-9ECB-47B0-9ED9-7E1C6BAB9523");
            classAudio.ClassIconClass = "xp-bubble";
            classAudio.ClassHasUnmanagedDbSchema = false;
            classAudio.ClassType = "Content";
            classAudio.ClassContentTypeType = "Reusable";
            classAudio.ClassWebPageHasUrl = true;
            classAudio.ClassShortName = "GenericAudio";

            var formInfo = existingClassAudio != null ? new FormInfo(existingClassAudio.ClassFormDefinition) : FormHelper.GetBasicFormDefinition("ContentItemDataID");

            // The main field will always exist, but check Guid value to make sure stays identical.
            var existingFieldContentItemDataID = formInfo.GetFormField("ContentItemDataID");
            var guidMatches = existingFieldContentItemDataID.Guid.Equals(Guid.Parse("8dc1c5d8-34ea-40c8-a94d-f2d26bedaaf8"));
            if (!guidMatches) {
                existingFieldContentItemDataID.Guid = Guid.Parse("8dc1c5d8-34ea-40c8-a94d-f2d26bedaaf8");
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
            fieldContentItemDataCommonDataID.Guid = Guid.Parse("44870b3e-fc93-498c-baa0-d4159f712bf9");

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
            fieldContentItemDataGUID.Guid = Guid.Parse("3195e28c-5876-4e19-bc4d-4e571199cc03");

            if (existingFieldContentItemDataGUID != null) {
                formInfo.UpdateFormField("ContentItemDataGUID", fieldContentItemDataGUID);
            } else {
                formInfo.AddFormItem(fieldContentItemDataGUID);
            }


            // Add or Update Title Field
            var existingFieldTitle = contentItemCommonDataForm.GetFormField(nameof(Audio.AudioTitle));
            var fieldTitle = existingFieldTitle ?? new FormFieldInfo();
            fieldTitle.Name = nameof(Audio.AudioTitle);
            fieldTitle.AllowEmpty = false;
            fieldTitle.Precision = 0;
            fieldTitle.Size = 100;
            fieldTitle.DataType = "text";
            fieldTitle.Enabled = true;
            fieldTitle.Visible = true;
            fieldTitle.Guid = Guid.Parse("bc2c5888-7a53-43d5-8180-fcb734cfb3ed");
            fieldTitle.SetComponentName("Kentico.Administration.TextInput");
            fieldTitle.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, "Title");
            fieldTitle.SetPropertyValue(FormFieldPropertyEnum.FieldDescriptionAsHtml, "False");
            fieldTitle.SetPropertyValue(FormFieldPropertyEnum.ExplanationTextAsHtml, "False");

            if (existingFieldTitle != null) {
                contentItemCommonDataForm.UpdateFormField(nameof(Audio.AudioTitle), fieldTitle);
            } else {
                contentItemCommonDataForm.AddFormItem(fieldTitle);
            }

            // Add or Update Description Field
            var existingFieldDescription = contentItemCommonDataForm.GetFormField(nameof(Audio.AudioDescription));
            var fieldDescription = existingFieldDescription ?? new FormFieldInfo();
            fieldDescription.Name = nameof(Audio.AudioDescription);
            fieldDescription.AllowEmpty = true;
            fieldDescription.Precision = 0;
            fieldDescription.DataType = "longtext";
            fieldDescription.Enabled = true;
            fieldDescription.Visible = true;
            fieldDescription.Guid = Guid.Parse("75951323-4aa5-4ddf-9673-a6cb19f9dbbe");
            fieldDescription.SetComponentName("Kentico.Administration.TextInput");
            fieldDescription.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, "Description");
            fieldDescription.SetPropertyValue(FormFieldPropertyEnum.FieldDescriptionAsHtml, "False");
            fieldDescription.SetPropertyValue(FormFieldPropertyEnum.ExplanationTextAsHtml, "False");

            if (existingFieldDescription != null) {
                contentItemCommonDataForm.UpdateFormField(nameof(Audio.AudioDescription), fieldDescription);
            } else {
                contentItemCommonDataForm.AddFormItem(fieldDescription);
            }

            // Add or Update File Field
            var existingFieldFile = contentItemCommonDataForm.GetFormField(nameof(Audio.AudioFile));
            var fieldFile = existingFieldFile ?? new FormFieldInfo();
            fieldFile.Name = nameof(Audio.AudioFile);
            fieldFile.AllowEmpty = true;
            fieldFile.Precision = 0;
            fieldFile.DataType = "contentitemasset";
            fieldFile.Enabled = true;
            fieldFile.Visible = true;
            fieldFile.Guid = Guid.Parse("188bc7cb-3110-45f3-abfb-e21ccfed3900");
            fieldFile.SetComponentName("Kentico.Administration.ContentItemAssetUploader");
            fieldFile.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, "File");
            fieldFile.SetPropertyValue(FormFieldPropertyEnum.FieldDescriptionAsHtml, "False");
            fieldFile.SetPropertyValue(FormFieldPropertyEnum.ExplanationTextAsHtml, "False");
            fieldFile.Settings.Add("AllowedExtensions", _startingSiteInstallationOptions.AudioFormatsSupported);
            fieldFile.ValidationRuleConfigurationsXmlData = @"<validationrulesdata><ValidationRuleConfiguration><ValidationRuleIdentifier>Kentico.Administration.RequiredValue</ValidationRuleIdentifier><RuleValues><ErrorMessage>Must add a file</ErrorMessage></RuleValues></ValidationRuleConfiguration></validationrulesdata>";

            if (existingFieldFile != null) {
                contentItemCommonDataForm.UpdateFormField(nameof(Audio.AudioFile), fieldFile);
            } else {
                contentItemCommonDataForm.AddFormItem(fieldFile);
            }

            // Add or Update Transcript Field
            var existingFieldTranscript = contentItemCommonDataForm.GetFormField(nameof(Audio.AudioTranscript));
            var fieldTranscript = existingFieldTranscript ?? new FormFieldInfo();
            fieldTranscript.Name = nameof(Audio.AudioTranscript);
            fieldTranscript.AllowEmpty = true;
            fieldTranscript.Precision = 0;
            fieldTranscript.DataType = "longtext";
            fieldTranscript.Enabled = true;
            fieldTranscript.Visible = true;
            fieldTranscript.Guid = Guid.Parse("48ab8938-6c36-41b5-b1ae-81393bc95f53");
            fieldTranscript.SetComponentName("Kentico.Administration.TextArea");
            fieldTranscript.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, "Transcript");
            fieldTranscript.SetPropertyValue(FormFieldPropertyEnum.FieldDescriptionAsHtml, "False");
            fieldTranscript.SetPropertyValue(FormFieldPropertyEnum.ExplanationTextAsHtml, "False");
            fieldTranscript.Settings.Add("MaxRowsNumber", 5);
            fieldTranscript.Settings.Add("MinRowsNumber", 3);

            if (existingFieldTranscript != null) {
                contentItemCommonDataForm.UpdateFormField(nameof(Audio.AudioTranscript), fieldTranscript);
            } else {
                contentItemCommonDataForm.AddFormItem(fieldTranscript);
            }

            classAudio.ClassFormDefinition = formInfo.GetXmlDefinition();
            if (classAudio.HasChanged) {
                DataClassInfoProvider.SetDataClassInfo(classAudio);
            }
        }

        private void CreateVideoContentType()
        {
            var contentItemCommonDataForm = FormHelper.GetFormInfo(ContentItemCommonDataInfo.OBJECT_TYPE, false);

            var existingClassVideo = DataClassInfoProvider.GetClasses().WhereEquals(nameof(DataClassInfo.ClassName), "Generic.Video")
                .GetEnumerableTypedResult().FirstOrDefault();
            var classVideo = existingClassVideo ?? DataClassInfo.New("Generic.Video");
            classVideo.ClassDisplayName = "Media File - Video";
            classVideo.ClassName = "Generic.Video";
            classVideo.ClassTableName = "Generic_Video";
            classVideo.ClassGUID = Guid.Parse("A88C3774-B14D-43D2-B150-D3BE1C1A86FA");
            classVideo.ClassIconClass = "xp-brand-youtube";
            classVideo.ClassHasUnmanagedDbSchema = false;
            classVideo.ClassType = "Content";
            classVideo.ClassContentTypeType = "Reusable";
            classVideo.ClassWebPageHasUrl = true;
            classVideo.ClassShortName = "GenericVideo";

            var formInfo = existingClassVideo != null ? new FormInfo(existingClassVideo.ClassFormDefinition) : FormHelper.GetBasicFormDefinition("ContentItemDataID");

            // The main field will always exist, but check Guid value to make sure stays identical.
            var existingFieldContentItemDataID = formInfo.GetFormField("ContentItemDataID");
            var guidMatches = existingFieldContentItemDataID.Guid.Equals(Guid.Parse("12f0dda1-368b-4979-b7f3-25a0018b5b0f"));
            if (!guidMatches) {
                existingFieldContentItemDataID.Guid = Guid.Parse("12f0dda1-368b-4979-b7f3-25a0018b5b0f");
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
            fieldContentItemDataCommonDataID.Guid = Guid.Parse("bca32608-9e68-4b62-98a2-c58ef23f363a");

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
            fieldContentItemDataGUID.Guid = Guid.Parse("15990174-666a-41cb-bd97-f333508709d3");

            if (existingFieldContentItemDataGUID != null) {
                formInfo.UpdateFormField("ContentItemDataGUID", fieldContentItemDataGUID);
            } else {
                formInfo.AddFormItem(fieldContentItemDataGUID);
            }


            // Add or Update Title Field
            var existingFieldTitle = contentItemCommonDataForm.GetFormField(nameof(Video.VideoTitle));
            var fieldTitle = existingFieldTitle ?? new FormFieldInfo();
            fieldTitle.Name = nameof(Video.VideoTitle);
            fieldTitle.AllowEmpty = false;
            fieldTitle.Precision = 0;
            fieldTitle.Size = 100;
            fieldTitle.DataType = "text";
            fieldTitle.Enabled = true;
            fieldTitle.Visible = true;
            fieldTitle.Guid = Guid.Parse("084941bb-aa75-4d40-969e-c7f7d0fa6b68");
            fieldTitle.SetComponentName("Kentico.Administration.TextInput");
            fieldTitle.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, "Title");
            fieldTitle.SetPropertyValue(FormFieldPropertyEnum.FieldDescriptionAsHtml, "False");
            fieldTitle.SetPropertyValue(FormFieldPropertyEnum.ExplanationTextAsHtml, "False");

            if (existingFieldTitle != null) {
                contentItemCommonDataForm.UpdateFormField(nameof(Video.VideoTitle), fieldTitle);
            } else {
                contentItemCommonDataForm.AddFormItem(fieldTitle);
            }

            // Add or Update Description Field
            var existingFieldDescription = contentItemCommonDataForm.GetFormField(nameof(Video.VideoDescription));
            var fieldDescription = existingFieldDescription ?? new FormFieldInfo();
            fieldDescription.Name = nameof(Video.VideoDescription);
            fieldDescription.AllowEmpty = true;
            fieldDescription.Precision = 0;
            fieldDescription.DataType = "longtext";
            fieldDescription.Enabled = true;
            fieldDescription.Visible = true;
            fieldDescription.Guid = Guid.Parse("ec672c1a-92cd-492c-8a37-e65ae32982c3");
            fieldDescription.SetComponentName("Kentico.Administration.TextInput");
            fieldDescription.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, "Description");
            fieldDescription.SetPropertyValue(FormFieldPropertyEnum.FieldDescriptionAsHtml, "False");
            fieldDescription.SetPropertyValue(FormFieldPropertyEnum.ExplanationTextAsHtml, "False");

            if (existingFieldDescription != null) {
                contentItemCommonDataForm.UpdateFormField(nameof(Video.VideoDescription), fieldDescription);
            } else {
                contentItemCommonDataForm.AddFormItem(fieldDescription);
            }

            // Add or Update File Field
            var existingFieldFile = contentItemCommonDataForm.GetFormField(nameof(Video.VideoFile));
            var fieldFile = existingFieldFile ?? new FormFieldInfo();
            fieldFile.Name = nameof(Video.VideoFile);
            fieldFile.AllowEmpty = true;
            fieldFile.Precision = 0;
            fieldFile.DataType = "contentitemasset";
            fieldFile.Enabled = true;
            fieldFile.Visible = true;
            fieldFile.Guid = Guid.Parse("05b5348d-16e4-439d-b620-75c40b4d7eb5");
            fieldFile.SetComponentName("Kentico.Administration.ContentItemAssetUploader");
            fieldFile.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, "File");
            fieldFile.SetPropertyValue(FormFieldPropertyEnum.FieldDescriptionAsHtml, "False");
            fieldFile.SetPropertyValue(FormFieldPropertyEnum.ExplanationTextAsHtml, "False");
            fieldFile.Settings.Add("AllowedExtensions", _startingSiteInstallationOptions.VideoFormatsSupported);
            fieldFile.ValidationRuleConfigurationsXmlData = @"<validationrulesdata><ValidationRuleConfiguration><ValidationRuleIdentifier>Kentico.Administration.RequiredValue</ValidationRuleIdentifier><RuleValues><ErrorMessage>Must add a file</ErrorMessage></RuleValues></ValidationRuleConfiguration></validationrulesdata>";

            if (existingFieldFile != null) {
                contentItemCommonDataForm.UpdateFormField(nameof(Video.VideoFile), fieldFile);
            } else {
                contentItemCommonDataForm.AddFormItem(fieldFile);
            }

            // Add or Update Transcript Field
            var existingFieldTranscript = contentItemCommonDataForm.GetFormField(nameof(Video.VideoTranscript));
            var fieldTranscript = existingFieldTranscript ?? new FormFieldInfo();
            fieldTranscript.Name = nameof(Video.VideoTranscript);
            fieldTranscript.AllowEmpty = true;
            fieldTranscript.Precision = 0;
            fieldTranscript.DataType = "longtext";
            fieldTranscript.Enabled = true;
            fieldTranscript.Visible = true;
            fieldTranscript.Guid = Guid.Parse("f7c0c3d3-a3e3-4af4-a6b6-af298d6f90ec");
            fieldTranscript.SetComponentName("Kentico.Administration.TextArea");
            fieldTranscript.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, "Transcript");
            fieldTranscript.SetPropertyValue(FormFieldPropertyEnum.FieldDescriptionAsHtml, "False");
            fieldTranscript.SetPropertyValue(FormFieldPropertyEnum.ExplanationTextAsHtml, "False");
            fieldTranscript.Settings.Add("MaxRowsNumber", 5);
            fieldTranscript.Settings.Add("MinRowsNumber", 3);

            if (existingFieldTranscript != null) {
                contentItemCommonDataForm.UpdateFormField(nameof(Video.VideoTranscript), fieldTranscript);
            } else {
                contentItemCommonDataForm.AddFormItem(fieldTranscript);
            }

            classVideo.ClassFormDefinition = formInfo.GetXmlDefinition();
            if (classVideo.HasChanged) {
                DataClassInfoProvider.SetDataClassInfo(classVideo);
            }
        }

        private void CreateBasicWebpage()
        {
            var contentItemCommonDataForm = FormHelper.GetFormInfo(ContentItemCommonDataInfo.OBJECT_TYPE, false);
            var baseMetaDataSchema = contentItemCommonDataForm.GetFormSchema("Base.Metadata");
            var redirectMetaDataSchema = contentItemCommonDataForm.GetFormSchema("Base.Redirect");
            var memberPermissionSchema = contentItemCommonDataForm.GetFormSchema("XperienceCommunity.MemberPermissionConfiguration");

            if (baseMetaDataSchema == null || redirectMetaDataSchema == null || memberPermissionSchema == null) {
                _eventLogService.LogError("BaselineModuleInstaller", "MissingSchemas", eventDescription: $"Can't create Basic page type because one of the following schemas are missing: {(baseMetaDataSchema == null ? "Base.Metadata" : "")} {(redirectMetaDataSchema == null ? "Base.Redirect" : "")} {(memberPermissionSchema == null ? "XperienceCommunity.MemberPermissionConfiguration" : "")}");
                return;
            }

            var existingAccountClass = DataClassInfoProvider.GetClasses().WhereEquals(nameof(DataClassInfo.ClassName), "Generic.BasicPage")
                .GetEnumerableTypedResult().FirstOrDefault();
            var accountClass = existingAccountClass ?? DataClassInfo.New("Generic.BasicPage");
            accountClass.ClassDisplayName = "Basic Page";
            accountClass.ClassName = "Generic.BasicPage";
            accountClass.ClassTableName = "Generic_BasicPage";
            accountClass.ClassGUID = Guid.Parse("9EF89D5B-C45B-4617-B16B-1BD2046F8A5E");
            accountClass.ClassIconClass = "xp-doc-inverted";
            accountClass.ClassHasUnmanagedDbSchema = false;
            accountClass.ClassType = "Content";
            accountClass.ClassContentTypeType = "Website";
            accountClass.ClassWebPageHasUrl = true;
            accountClass.ClassShortName = "GenericBasicPage";

            var formInfo = existingAccountClass != null ? new FormInfo(existingAccountClass.ClassFormDefinition) : FormHelper.GetBasicFormDefinition("ContentItemDataID");

            // The main field will always exist, but check Guid value to make sure stays identical.
            var existingFieldContentItemDataID = formInfo.GetFormField("ContentItemDataID");
            var guidMatches = existingFieldContentItemDataID.Guid.Equals(Guid.Parse("b964fdbd-b5d8-4fe0-8a41-7959bfc011dd"));
            if (!guidMatches) {
                existingFieldContentItemDataID.Guid = Guid.Parse("b964fdbd-b5d8-4fe0-8a41-7959bfc011dd");
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
            fieldContentItemDataCommonDataID.Guid = Guid.Parse("2b7034d0-ad21-4de0-b021-e8a94d265e9e");

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
            fieldContentItemDataGUID.Guid = Guid.Parse("2e5b092b-1f92-4ca6-9ce5-dcff9180deab");

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

        private void CreateHomeWebpage()
        {
            var contentItemCommonDataForm = FormHelper.GetFormInfo(ContentItemCommonDataInfo.OBJECT_TYPE, false);
            var baseMetaDataSchema = contentItemCommonDataForm.GetFormSchema("Base.Metadata");
            var redirectMetaDataSchema = contentItemCommonDataForm.GetFormSchema("Base.Redirect");

            if (baseMetaDataSchema == null || redirectMetaDataSchema == null) {
                _eventLogService.LogError("BaselineModuleInstaller", "MissingSchemas", eventDescription: $"Can't create Home page type because one of the following schemas are missing: {(baseMetaDataSchema == null ? "Base.Metadata" : "")} {(redirectMetaDataSchema == null ? "Base.Redirect" : "")}");
                return;
            }

            var existingAccountClass = DataClassInfoProvider.GetClasses().WhereEquals(nameof(DataClassInfo.ClassName), "Generic.Home")
                .GetEnumerableTypedResult().FirstOrDefault();
            var accountClass = existingAccountClass ?? DataClassInfo.New("Generic.Home");
            accountClass.ClassDisplayName = "Home";
            accountClass.ClassName = "Generic.Home";
            accountClass.ClassTableName = "Generic_Home";
            accountClass.ClassGUID = Guid.Parse("ABBBAB45-6764-4B61-AFF3-098531CC7565");
            accountClass.ClassIconClass = "xp-home";
            accountClass.ClassHasUnmanagedDbSchema = false;
            accountClass.ClassType = "Content";
            accountClass.ClassContentTypeType = "Website";
            accountClass.ClassWebPageHasUrl = true;
            accountClass.ClassShortName = "GenericHome";

            var formInfo = existingAccountClass != null ? new FormInfo(existingAccountClass.ClassFormDefinition) : FormHelper.GetBasicFormDefinition("ContentItemDataID");

            // The main field will always exist, but check Guid value to make sure stays identical.
            var existingFieldContentItemDataID = formInfo.GetFormField("ContentItemDataID");
            var guidMatches = existingFieldContentItemDataID.Guid.Equals(Guid.Parse("7bbb48a6-6852-4b52-993c-e5c9ade87125"));
            if (!guidMatches) {
                existingFieldContentItemDataID.Guid = Guid.Parse("7bbb48a6-6852-4b52-993c-e5c9ade87125");
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
            fieldContentItemDataCommonDataID.Guid = Guid.Parse("ec0edd81-7ac9-4d1f-b49f-a2fc23dca8e8");

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
            fieldContentItemDataGUID.Guid = Guid.Parse("eb525402-d63d-4609-b69b-7b36a4e3b590");

            if (existingFieldContentItemDataGUID != null) {
                formInfo.UpdateFormField("ContentItemDataGUID", fieldContentItemDataGUID);
            } else {
                formInfo.AddFormItem(fieldContentItemDataGUID);
            }

            // Add the 2 schemas
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
