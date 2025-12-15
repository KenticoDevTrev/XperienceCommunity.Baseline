using CMS.Core;

namespace Core.Services.Implementation
{
    [Obsolete("Kentico now implements the system ILogger which you can start using, or the IEventLogService.")]
    public class Logger(IEventLogService eventLogService) : ILogger
    {
        private readonly IEventLogService _eventLogService = eventLogService;

        public void LogException(Exception ex, string Source, string EventCode, string Description = "")
        {
            _eventLogService.LogException(Source, EventCode, ex, additionalMessage: Description);
        }

        public void LogException(string Source, string EventCode, string Description = "")
        {
            _eventLogService.LogEvent(EventTypeEnum.Error, Source, EventCode, eventDescription: Description);
        }

        public void LogInformation(string Source, string EventCode, string Description = "")
        {
            _eventLogService.LogInformation(Source, EventCode, Description);
        }

        public void LogWarning(Exception ex, string Source, string EventCode, string Description = "")
        {
            _eventLogService.LogWarning(Source, EventCode, ex, Description);
        }
    }
}
