﻿namespace Core.Services
{
    /// <summary>
    /// Agnostic Event Log Service.
    /// </summary>
    public interface ILogger
    {
        void LogException(string Source, string EventCode, string Description = "");
        
        void LogException(Exception ex, string Source, string EventCode, string Description = "");

        void LogWarning(Exception ex, string Source, string EventCode, string Description = "");

        void LogInformation(string Source, string EventCode, string Description = "");

    }
}