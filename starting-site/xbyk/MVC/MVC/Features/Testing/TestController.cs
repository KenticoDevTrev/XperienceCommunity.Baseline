using Core.Interfaces;
using Core.Repositories;
using Core.Services;
namespace MVC.Features.Testing
{
    public class TestController(
        ILogger Logger,
ISiteRepository SiteRepository,
IUserRepository UserRepository,
IUserMetadataProvider UserMetadataProvider,
IUrlResolver UrlResolver,
IIdentityService IdentityService,
IPageIdentityFactory PageIdentityFactory,
IBaselinePageBuilderContext BaselinePageBuilderContext,
ICategoryCachedRepository CategoryCachedRepository,
IMediaRepository MediaRepository,
IMediaFileMediaMetadataProvider MediaFileMediaMetadataProvider,
IContentItemMediaCustomizer ContentItemMediaCustomizer,
IContentItemMediaMetadataQueryEditor ContentItemMediaMetadataQueryEditor,
IContentCategoryRepository ContentCategoryRepository,
ICustomTaxonomyFieldParser CustomTaxonomyFieldParser,
ILanguageFallbackRepository LanguageFallbackRepository,
IPageContextRepository PageContextRepository,
#pragma warning disable CS0618 // Type or member is obsolete
IPageCategoryRepository PageCategoryRepository
#pragma warning restore CS0618 // Type or member is obsolete


        ) : Controller
    {
        private readonly ILogger _logger = Logger;
        private readonly ISiteRepository _siteRepository = SiteRepository;
        private readonly IUserRepository _userRepository = UserRepository;
        private readonly IUserMetadataProvider _userMetadataProvider = UserMetadataProvider;
        private readonly IUrlResolver _urlResolver = UrlResolver;
        private readonly IIdentityService _identityService = IdentityService;
        private readonly IPageIdentityFactory _pageIdentityFactory = PageIdentityFactory;
        private readonly IBaselinePageBuilderContext _baselinePageBuilderContext = BaselinePageBuilderContext;
        private readonly ICategoryCachedRepository _categoryCachedRepository = CategoryCachedRepository;
        private readonly IMediaRepository _mediaRepository = MediaRepository;
        private readonly IMediaFileMediaMetadataProvider _mediaFileMediaMetadataProvider = MediaFileMediaMetadataProvider;
        private readonly IContentItemMediaCustomizer _contentItemMediaCustomizer = ContentItemMediaCustomizer;
        private readonly IContentItemMediaMetadataQueryEditor _contentItemMediaMetadataQueryEditor = ContentItemMediaMetadataQueryEditor;
        private readonly IContentCategoryRepository _contentCategoryRepository = ContentCategoryRepository;
        private readonly ICustomTaxonomyFieldParser _customTaxonomyFieldParser = CustomTaxonomyFieldParser;
        private readonly ILanguageFallbackRepository _languageFallbackRepository = LanguageFallbackRepository;
        private readonly IPageContextRepository _pageContextRepository = PageContextRepository;
#pragma warning disable CS0618 // Type or member is obsolete
        private readonly IPageCategoryRepository _pageCategoryRepository = PageCategoryRepository;
#pragma warning restore CS0618 // Type or member is obsolete

        public Task<string> Index()
        {

            
            return Task.FromResult(string.Empty);
        }

    }
}
