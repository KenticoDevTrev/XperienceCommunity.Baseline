using CMS.ContentEngine;
using CMS.ContentEngine.Internal;
using CMS.DataEngine;
using CMS.MediaLibrary;
using CMS.Membership;
using CMS.Websites;
using Core.Interfaces;
using Core.Repositories.Implementation;
using Core.Services;
using Generic;
using MVCCaching;
using System.Data;
using System.Net.Http;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace MVC.Features.Testing
{
    
    public class TestController(ILinkedItemsDependencyAsyncRetriever linkedItemsDependencyAsyncRetriever, IWebPageLinkedItemsDependencyRetriever webPageLinkedItemsDependencyRetriever,
        IContentQueryExecutor contentQueryExecutor,
        ICachingReferenceService cachingReferenceService,
        IHttpClientFactory HttpClientFactory,
        IProgressiveCache progressiveCache,
        IUrlResolver urlResolver,
        IContentQueryResultMapper contentQueryResultMapper
        
        ) : Controller
    {
        public ILinkedItemsDependencyAsyncRetriever LinkedItemsDependencyAsyncRetriever { get; } = linkedItemsDependencyAsyncRetriever;
        public IWebPageLinkedItemsDependencyRetriever WebPageLinkedItemsDependencyRetriever { get; } = webPageLinkedItemsDependencyRetriever;
        public IContentQueryExecutor ContentQueryExecutor { get; } = contentQueryExecutor;
        public ICachingReferenceService CachingReferenceService { get; } = cachingReferenceService;
        public IProgressiveCache ProgressiveCache { get; } = progressiveCache;
        public IUrlResolver UrlResolver { get; } = urlResolver;
        public IContentQueryResultMapper ContentQueryResultMapper { get; } = contentQueryResultMapper;

        public async Task<string> SvgTest()
        {
            var imageBytes = await HttpClientFactory.CreateClient().GetByteArrayAsync("https://www.gstatic.com/webp/gallery/1.webp");
            var widthHeight = ImageHelper.GetImageDimensions(imageBytes);
            if (widthHeight.width == 0 && widthHeight.height == 0) {
                var result = Result.Failure<IMediaMetadata>("Could not parse width height from image bytes");
            }
            var test = new MediaMetadataImage(widthHeight.width, widthHeight.height);

            


            return string.Empty;
        }

        public async Task<string> Index()
        {

            var results = await ContentQueryExecutor.GetMappedResult<File>(new ContentItemQueryBuilder().ForContentType(Generic.File.CONTENT_TYPE_NAME, subQuery => subQuery.WithLinkedItems(1)));
            var url = results.First().FileAttachment.Url;
            
            var fileType = "Generic.File";
            var fileAttachmentField = "FileAttachment";
            var fileTitleField = "FileName";
            var fileDescriptionField = "FileDescription";
            Maybe<IContentItemMediaMetadataProvider> metaDataProvider = new CustomContentItemMediaMetadataProvider(ProgressiveCache, UrlResolver, HttpClientFactory, ContentQueryResultMapper);
           
            string attachmentFieldGuid = string.Empty;

            // Add to Cache
            var classes = await DataClassInfoProvider.GetClasses()
               .Columns(nameof(DataClassInfo.ClassName), nameof(DataClassInfo.ClassFormDefinition))
               .WhereLike(nameof(DataClassInfo.ClassFormDefinition), "%contentitemasset%")
               .GetEnumerableTypedResultAsync();

            var typeToFieldToGuid = classes
                .ToDictionary(key => key.ClassName.ToLowerInvariant(), value => {
                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(value.ClassFormDefinition);
                    return xmlDoc.SelectNodes("//field[@columntype='contentitemasset']")
                                    .Cast<XmlNode>()
                                    .Select(x => new ContentItemAttachmentColumnAndGuid(x.Attributes["column"].Value.ToLowerInvariant(),  Guid.Parse(x.Attributes["guid"].Value)))
                                    .ToDictionary(key => key.AttachmentColumnName, value => value.FieldGuidValue);
                });

            if(typeToFieldToGuid.TryGetValue(fileType.ToLowerInvariant(), out var fieldToGuid)
                && fieldToGuid.TryGetValue(fileAttachmentField.ToLowerInvariant(), out var attachmentField)) {
                attachmentFieldGuid = attachmentField.ToString().ToLower();

                var queryBuilder = new ContentItemQueryBuilder().ForContentType(fileType, subQuery =>
                   subQuery.Columns("ContentItemGuid", "ContentItemCommonDataContentLanguageID", fileAttachmentField, fileTitleField, fileDescriptionField)
                  );

                var result = await ContentQueryExecutor.GetResult(queryBuilder, async (fileItem) => {
                    
                    var metaData = System.Text.Json.JsonSerializer.Deserialize<ContentItemAssetMetadata>(fileItem.GetValue<string>(fileAttachmentField));
                    var title = fileItem.GetValue<string>(fileTitleField);
                    var description = fileItem.GetValue<string>(fileDescriptionField).AsNullOrWhitespaceMaybe();
                    var language = CachingReferenceService.GetLanguageNameById(fileItem.ContentItemCommonDataContentLanguageID);

                    var permanentUrl = $"/getcontentasset/{fileItem.ContentItemGUID}/{attachmentFieldGuid}/{metaData.Name}{metaData.Extension}?language={language}";
                    var directUrl = $"/getcontentasset/{fileItem.ContentItemGUID}/{attachmentFieldGuid}/{metaData.Name}{metaData.Extension}?language={language}";
                    //var directUrl = $"/assets/contentitems/{fileItem.ContentItemGUID.ToString()[..2]}/{attachmentFieldGuid}/{metaData.Identifier}{metaData.Extension}".ToLower();

                    var mediaItem = new MediaItem(fileItem.ContentItemGUID, metaData.Name, title, metaData.Extension, directUrl, permanentUrl) {
                        MediaDescription = description
                    };
                    if(metaDataProvider.TryGetValue(out var metaDataProviderVal) && (await metaDataProviderVal.GetMediaMetadata(fileItem, metaData, mediaItem)).TryGetValue(out var metaDataItemResult)) {
                        mediaItem = mediaItem with { MetaData = Maybe.From(metaDataItemResult) };
                    }
                    return mediaItem;
                });
            }

            

            return string.Empty;
        }

        public record ContentItemAttachmentColumnAndGuid(string AttachmentColumnName, Guid FieldGuidValue);
    }
}
