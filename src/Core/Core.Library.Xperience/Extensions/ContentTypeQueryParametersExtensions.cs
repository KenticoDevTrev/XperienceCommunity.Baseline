using CMS.ContentEngine.Internal;
using CMS.Websites;

namespace CMS.ContentEngine
{
    public static class ContentTypeQueryParametersExtensions
    {
        public static ContentTypeQueryParameters InContentIdentities(this ContentTypeQueryParameters queryParams, IEnumerable<ContentIdentity> identities)
        {
            var ids = identities.Where(x => x.ContentID.HasValue).Select(x => x.ContentID.Value);
            var guids = identities.Where(x => x.ContentGuid.HasValue).Select(x => x.ContentGuid.Value);
            var names = identities.Where(x => x.ContentName.HasValue).Select(x => x.ContentName.Value);

            if (ids.Count() == identities.Count()) {
                queryParams.Where(where => where.WhereIn(nameof(ContentItemInfo.ContentItemID), ids));
                return queryParams;
            }
            if (guids.Count() == identities.Count()) {
                queryParams.Where(where => where.WhereIn(nameof(ContentItemInfo.ContentItemGUID), guids));
                return queryParams;
            }
            if (names.Count() == identities.Count()) {
                queryParams.Where(where => where.WhereIn(nameof(ContentItemInfo.ContentItemName), names));
                return queryParams;
            }

            // must be a mixture
            if (ids.Any() || guids.Any() || names.Any()) {
                queryParams.Where(where => {
                    var whereSetYet = false;
                    if (ids.Any()) {
                        if (whereSetYet) {
                            where.Or();
                        }
                        where.WhereIn(nameof(ContentItemInfo.ContentItemID), ids);
                        whereSetYet = true;
                    }
                    if (guids.Any()) {
                        if (whereSetYet) {
                            where.Or();
                        }
                        where.WhereIn(nameof(ContentItemInfo.ContentItemGUID), guids);
                        whereSetYet = true;
                    }
                    if (names.Any()) {
                        if (whereSetYet) {
                            where.Or();
                        }
                        where.WhereIn(nameof(ContentItemInfo.ContentItemName), names);
                        whereSetYet = true;
                    }
                });
            }

            return queryParams;
        }

        public static ContentTypeQueryParameters InContentIdentity(this ContentTypeQueryParameters queryParams, ContentIdentity identity) => queryParams.InContentIdentities([identity]);

        public static ContentTypeQueryParameters InContentCultureIdentities(this ContentTypeQueryParameters queryParams, IEnumerable<ContentCultureIdentity> identities)
        {
            var ids = identities.Where(x => x.ContentCultureID.HasValue).Select(x => x.ContentCultureID.Value);
            var guids = identities.Where(x => x.ContentCultureGuid.HasValue).Select(x => x.ContentCultureGuid.Value);
            var lookups = identities.Where(x => x.ContentCultureLookup.HasValue).Select(x => x.ContentCultureLookup.Value);

            if (ids.Count() == identities.Count()) {
                queryParams.Where(where => where.WhereIn(nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataID), ids));
                return queryParams;
            }
            if (guids.Count() == identities.Count()) {
                queryParams.Where(where => where.WhereIn(nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataGUID), guids));
                return queryParams;
            }
            if (lookups.Count() == identities.Count()) {
                queryParams.Where(where => {
                    where.WhereIn(nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataContentItemID), lookups.Select(x => x.ContentId));
                });
                return queryParams;
            }

            // must be a mixture
            if (ids.Any() || guids.Any() || lookups.Any()) {
                queryParams.Where(where => {
                    var whereSetYet = false;
                    if (ids.Any()) {
                        if (whereSetYet) {
                            where.Or();
                        }
                        where.WhereIn(nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataID), ids);
                        whereSetYet = true;
                    }
                    if (guids.Any()) {
                        if (whereSetYet) {
                            where.Or();
                        }
                        where.WhereIn(nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataGUID), guids);
                        whereSetYet = true;
                    }
                    if (lookups.Any()) {
                        if (whereSetYet) {
                            where.Or();
                        }
                        where.WhereIn(nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataContentItemID), lookups.Select(x => x.ContentId));
                        whereSetYet = true;
                    }
                });
            }

            return queryParams;
        }
        
        public static ContentTypeQueryParameters InContentCultureIdentity(this ContentTypeQueryParameters queryParams, ContentCultureIdentity identity) => queryParams.InContentCultureIdentities([identity]);

        public static ContentTypeQueryParameters InTreeIdentities(this ContentTypeQueryParameters queryParams, IEnumerable<TreeIdentity> identities)
        {
            var ids = identities.Where(x => x.PageID.HasValue).Select(x => x.PageID.Value);
            var guids = identities.Where(x => x.PageGuid.HasValue).Select(x => x.PageGuid.Value);
            var names = identities.Where(x => x.PageName.HasValue).Select(x => x.PageName.Value);
            var lookups = identities.Where(x => x.PathChannelLookup.HasValue).Select(x => x.PathChannelLookup.Value);

            if (ids.Count() == identities.Count()) {
                queryParams.Where(where => where.WhereIn(nameof(WebPageFields.WebPageItemID), ids));
                return queryParams;
            }
            if (guids.Count() == identities.Count()) {
                queryParams.Where(where => where.WhereIn(nameof(WebPageFields.WebPageItemGUID), guids));
                return queryParams;
            }
            if (names.Count() == identities.Count()) {
                queryParams.Where(where => where.WhereIn(nameof(WebPageFields.WebPageItemName), names));
                return queryParams;
            }
            if (lookups.Count() == identities.Count()) {
                // Split paths and channels if multiple, really shouldn't be doing stuff like that though.
                var pathByChannel = lookups.GroupBy(key => key.ChannelId.GetValueOrDefault(0)).ToDictionary(key => key.Key, value => value.Select(x => x.Path));
                var subWhereSetYet = false;
                queryParams.Where(subWhere => {
                    foreach (var channelId in pathByChannel.Keys) {
                        if (subWhereSetYet)
                        {
                            subWhere.Or();
                        }
                        if (channelId > 0) {
                            subWhere.Where(where => where.WhereIn(nameof(WebPageFields.WebPageItemTreePath), pathByChannel[channelId].Select(x => x)).And().WhereEquals(nameof(WebPageFields.WebPageItemWebsiteChannelId), channelId));
                        } else {
                            subWhere.Where(where => where.WhereIn(nameof(WebPageFields.WebPageItemTreePath), pathByChannel[channelId].Select(x => x)));
                        }
                    }
                });
                return queryParams;
            }

            // must be a mixture
            if (ids.Any() || guids.Any() || names.Any()) {
                queryParams.Where(where => {
                    var whereSetYet = false;
                    if (ids.Any()) {
                        if (whereSetYet) {
                            where.Or();
                        }
                        where.WhereIn(nameof(WebPageFields.WebPageItemID), ids);
                        whereSetYet = true;
                    }
                    if (guids.Any()) {
                        if (whereSetYet) {
                            where.Or();
                        }
                        where.WhereIn(nameof(WebPageFields.WebPageItemGUID), guids);
                        whereSetYet = true;
                    }
                    if (names.Any()) {
                        if (whereSetYet) {
                            where.Or();
                        }
                        where.WhereIn(nameof(WebPageFields.WebPageItemName), names);
                        whereSetYet = true;
                    }
                    if (lookups.Any()) {
                        if (whereSetYet) {
                            where.Or();
                        }
                        // Split paths and channels if multiple, really shouldn't be doing stuff like that though.
                        var pathByChannel = lookups.GroupBy(key => key.ChannelId.GetValueOrDefault(0)).ToDictionary(key => key.Key, value => value.Select(x => x.Path));
                        where.Where(subWhere => {
                            var subWhereSetYet = false;
                            foreach (var channelId in pathByChannel.Keys) {
                                if (subWhereSetYet) {
                                    subWhere.Or();
                                }
                                if (channelId > 0) {
                                    subWhere.Where(where => where.WhereIn(nameof(WebPageFields.WebPageItemTreePath), pathByChannel[channelId].Select(x => x)).And().WhereEquals(nameof(WebPageFields.WebPageItemWebsiteChannelId), channelId));
                                    subWhereSetYet = true;
                                } else {
                                    subWhere.Where(where => where.WhereIn(nameof(WebPageFields.WebPageItemTreePath), pathByChannel[channelId].Select(x => x)));
                                    subWhereSetYet = true;
                                }
                            }
                        });
                        whereSetYet = true;
                    }
                });
            }

            return queryParams;
        }
        
        public static ContentTypeQueryParameters InTreeIdentity(this ContentTypeQueryParameters queryParams, TreeIdentity identity) => queryParams.InTreeIdentities([identity]);

        public static ContentTypeQueryParameters InTreeCultureIdentities(this ContentTypeQueryParameters queryParams, IEnumerable<TreeCultureIdentity> identities) => queryParams.InTreeIdentities(identities);
        
        public static ContentTypeQueryParameters InTreeCultureIdentity(this ContentTypeQueryParameters queryParams, TreeIdentity identity) => queryParams.InTreeIdentities([identity]);

        [Obsolete("No longer needed, base .Columns now handles if there are or aren't existing columns listings")]
        public static ContentTypeQueryParameters ColumnsSafe(this ContentTypeQueryParameters queryParams, params string[] columns) => queryParams.Columns(columns);

        public static ContentTypeQueryParameters Path(this ContentTypeQueryParameters queryParams, string path, PathTypeEnum type = PathTypeEnum.Explicit)
        {
            var pathNormalized = $"/{path.Trim('%').Trim('/')}";
            return type switch {
                PathTypeEnum.Section => queryParams.Where(where => where.WhereEquals(nameof(WebPageFields.WebPageItemTreePath), pathNormalized)
                                    .Or()
                                    .WhereLike(nameof(WebPageFields.WebPageItemTreePath), $"{pathNormalized}/%")),
                PathTypeEnum.Single => queryParams.Where(where => where.WhereEquals(nameof(WebPageFields.WebPageItemTreePath), pathNormalized)),
                PathTypeEnum.Children => queryParams.Where(where => where.WhereLike(nameof(WebPageFields.WebPageItemTreePath), $"{pathNormalized}/%")),
                _ or PathTypeEnum.Explicit => queryParams.Where(where => where.WhereEquals(nameof(WebPageFields.WebPageItemTreePath), path)),
            };
        }
    }
}
