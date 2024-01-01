using Core.Components.ConfigurationHelper;

namespace Core.Features.PageBuilderError
{
    public record PageBuilderErrorViewModel
    {
        public PageBuilderErrorViewModel(string message, bool inline, bool isError)
        {
            Message = message;
            if (inline)
            {
                Mode = ConfigurationHelperMode.Inline;
            }
            else
            {
                Mode = ConfigurationHelperMode.ToolTip;
            }
            NeedsAttention = isError;
        }
        public string Message { get; init; }
        public ConfigurationHelperMode Mode { get; init; }
        public bool NeedsAttention { get; init; }
    }
}
