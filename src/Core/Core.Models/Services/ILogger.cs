namespace Core.Services
{
    /// <summary>
    /// Agnostic Event Log Service.
    /// </summary>
    [Obsolete("Xperience now implements ILogger natively, so you can use the nagive ILogger from the system. This class will remain for KX13 usage.")]
    public interface ILogger
    {
        void LogException(string Source, string EventCode, string Description = "");
        
        void LogException(Exception ex, string Source, string EventCode, string Description = "");

        void LogWarning(Exception ex, string Source, string EventCode, string Description = "");

        void LogInformation(string Source, string EventCode, string Description = "");

    }
}