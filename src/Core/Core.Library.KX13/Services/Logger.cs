using CMS.Base;
using CMS.Core;

namespace Core.Services.Implementation
{
    [AutoDependencyInjection]
    public class Logger(ISiteService _siteRepo, IEventLogService _logService) : ILogger
    {
        public void LogException(Exception ex, string Source, string EventCode, string Description = "")
        {
            _logService.LogException(Source, EventCode, ex, additionalMessage: Description);
        }

        public void LogException(string Source, string EventCode, string Description = "")
        {
            _logService.LogEvent(EventTypeEnum.Error, Source, EventCode, eventDescription: Description);
        }

        public void LogInformation(string Source, string EventCode, string Description = "")
        {
            _logService.LogInformation(Source, EventCode, Description);
        }

        public void LogWarning(Exception ex, string Source, string EventCode, string Description = "")
        {
            _logService.LogWarning(Source, EventCode, ex, _siteRepo.CurrentSite.SiteID, Description);
        }
    }
}