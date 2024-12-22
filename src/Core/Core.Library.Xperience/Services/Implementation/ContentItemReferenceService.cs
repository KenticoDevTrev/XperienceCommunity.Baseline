using System.Text.Json;

namespace Core.Services.Implementation
{
    public class ContentItemReferenceService : IContentItemReferenceService
    {
        public IEnumerable<ContentItemReference> GetContentItemReferences(IContentQueryDataContainer itemData, string columnName)
        {
            try {
                var data = itemData.GetValue<string>(columnName);
                if(!string.IsNullOrWhiteSpace(data)) {
                    var items = JsonSerializer.Deserialize<ContentItemReference[]>(data);
                    if (items != null) {
                        return items;
                    }
                }
            } catch { }
            return [];
        }
    }
}
