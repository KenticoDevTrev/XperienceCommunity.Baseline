using CMS.ContentEngine.Internal;
using System.Xml;

namespace Core.Repositories.Implementation
{
    public class ClassContentTypeAssetConfigurationRepository(IProgressiveCache progressiveCache, 
        ContentItemAssetOptions contentItemAssetOptions) : IClassContentTypeAssetConfigurationRepository
    {
        private readonly IProgressiveCache _progressiveCache = progressiveCache;
        private readonly ContentItemAssetOptions _contentItemAssetOptions = contentItemAssetOptions;

        public async Task<Dictionary<string, ContentTypeAssetConfigurations>> GetClassNameToContentTypeAssetConfigurationDictionary()
        {
            return await _progressiveCache.LoadAsync(async cs => {

                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency([$"{DataClassInfo.OBJECT_TYPE}|all"]);
                }

                // Add to Cache
                var classes = await DataClassInfoProvider.GetClasses()
                   .Columns(nameof(DataClassInfo.ClassName), nameof(DataClassInfo.ClassFormDefinition))
                   .WhereLike(nameof(DataClassInfo.ClassFormDefinition), "%contentitemasset%")
                   .GetEnumerableTypedResultAsync();

                var typeToFieldToGuid = classes
                    .ToDictionary(key => key.ClassName.ToLowerInvariant(), value => {
                        var xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(value.ClassFormDefinition);
                        var nodes = xmlDoc.SelectNodes("//field[@columntype='contentitemasset']")?.Cast<XmlNode>() ?? [];
                        var nodeList = nodes.Where(x => x != null && x.Attributes != null && (x.Attributes["column"] != null && x.Attributes["guid"] != null));
#pragma warning disable CS8602 // Dereference of a possibly null reference. - Ensured above that this will not be the case
                        return nodeList.Select(x => new ContentItemAttachmentColumnAndGuid(x.Attributes["column"].Value.ToLowerInvariant(), Guid.Parse(x.Attributes["guid"].Value)))
                                    .ToDictionary(key => key.AttachmentColumnName.ToLowerInvariant(), value => value.FieldGuidValue) ?? [];
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                    });

                // combine with any configurations the user has provided
                var configuration = new Dictionary<string, ContentTypeAssetConfigurations>();

                foreach(var type in typeToFieldToGuid.Keys) {
                    var assetFieldIdentifiers = new List<AssetFieldIdentifierWithType>();
                    var assetOptionConfig = _contentItemAssetOptions.ContentItemConfigurations.FirstOrMaybe(x => x.ClassName.Equals(type, StringComparison.OrdinalIgnoreCase));
                    var fieldConfigDictionary = assetOptionConfig.TryGetValue(out var config) ? config.AssetFieldIdentifierConfigurations.ToDictionary(key => key.AssetFieldName.ToLowerInvariant(), value => value) : [];
                    foreach(var fieldName in typeToFieldToGuid[type].Keys) {
                        if(fieldConfigDictionary.TryGetValue(fieldName.ToLowerInvariant(), out var assetFieldConfig)) {
                            assetFieldIdentifiers.Add(
                                new AssetFieldIdentifierWithType(
                                    mediaType: assetFieldConfig.MediaType,
                                    assetFieldName: fieldName,
                                    fieldGuid: typeToFieldToGuid[type][fieldName],
                                    titleFieldName: assetFieldConfig.TitleFieldName.GetValueOrDefault(nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataDisplayName)),
                                    descriptionFieldName: assetFieldConfig.DescriptionFieldName.AsNullableValue()
                                    )
                                );
                        } else {
                            assetFieldIdentifiers.Add(
                               new AssetFieldIdentifierWithType(
                                   mediaType: ContentItemAssetMediaType.Unknown,
                                   assetFieldName: fieldName,
                                   fieldGuid: typeToFieldToGuid[type][fieldName],
                                   titleFieldName: nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataDisplayName),
                                   descriptionFieldName: null
                                   )
                               );
                        }
                    }
                    configuration.TryAdd(type.ToLowerInvariant(), new ContentTypeAssetConfigurations(assetFieldIdentifiers, assetOptionConfig.TryGetValue(out var configVal) && configVal.PreCache));
                }
                return configuration;
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetClassNameToFieldNameToAssetFieldIdentifierDictionary"));
        }

        public record ContentItemAttachmentColumnAndGuid(string AttachmentColumnName, Guid FieldGuidValue);
       
    }
}
