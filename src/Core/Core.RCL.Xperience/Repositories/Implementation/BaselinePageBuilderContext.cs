using CMS.Websites.Routing;
using Core.Models;
using Kentico.PageBuilder.Web.Mvc;

namespace Core.Repositories.Implementation
{
    public class BaselinePageBuilderContext(IWebsiteChannelContext websiteChannelContext, IPageBuilderDataContext pageBuilderDataContext) : IBaselinePageBuilderContext
    {
        public bool IsPreviewMode => WebsiteChannelContext.IsPreview;

        public bool IsLiveMode => !WebsiteChannelContext.IsPreview;

        public bool IsLivePreviewMode => WebsiteChannelContext.IsPreview && !PageBuilderDataContext.EditMode;

        public bool IsEditMode => PageBuilderDataContext.EditMode;

        public BaselinePageBuilderMode Mode => PageBuilderDataContext.EditMode ? BaselinePageBuilderMode.Edit : (WebsiteChannelContext.IsPreview ? BaselinePageBuilderMode.LivePreview : BaselinePageBuilderMode.Live);
        
        public IWebsiteChannelContext WebsiteChannelContext { get; } = websiteChannelContext;
        public IPageBuilderDataContext PageBuilderDataContext { get; } = pageBuilderDataContext;

        public string ModeName() => Mode.ToString();
    }
}
