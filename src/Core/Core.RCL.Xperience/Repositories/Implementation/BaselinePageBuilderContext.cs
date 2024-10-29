using CMS.Websites.Routing;
using Core.Models;
using Kentico.PageBuilder.Web.Mvc;

namespace Core.Repositories.Implementation
{
    public class BaselinePageBuilderContext(IWebsiteChannelContext websiteChannelContext, IPageBuilderDataContextRetriever pageBuilderDataContextRetriever) : IBaselinePageBuilderContext
    {
        public bool IsPreviewMode => WebsiteChannelContext.IsPreview;

        public bool IsLiveMode => !WebsiteChannelContext.IsPreview;

        public bool IsLivePreviewMode => WebsiteChannelContext.IsPreview && !PageBuilderDataContextRetriever.Retrieve().EditMode;

        public bool IsEditMode => PageBuilderDataContextRetriever.Retrieve().EditMode;

        public BaselinePageBuilderMode Mode => PageBuilderDataContextRetriever.Retrieve().EditMode ? BaselinePageBuilderMode.Edit : (WebsiteChannelContext.IsPreview ? BaselinePageBuilderMode.LivePreview : BaselinePageBuilderMode.Live);
        
        public IWebsiteChannelContext WebsiteChannelContext { get; } = websiteChannelContext;
        public IPageBuilderDataContextRetriever PageBuilderDataContextRetriever { get; } = pageBuilderDataContextRetriever;

        public string ModeName() => Mode.ToString();
    }
}
