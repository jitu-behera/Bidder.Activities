namespace RequestResponseLogger.Configuration
{
    public class LoggingConfiguration
    {
        public string ApplicationName { get; }
        public string ApplicationVersion { get; }
        public bool IsLoggingOn { get; }
        public string[] ExcludeLogRoutes { get; }

        public LoggingConfiguration(string applicationName, string applicationVersion,bool isLoggingOn,string[] excludeLogRoutes)
        {
            ApplicationName = applicationName;
            ApplicationVersion = applicationVersion;
            ExcludeLogRoutes = excludeLogRoutes;
            IsLoggingOn = isLoggingOn;
        }


    }
}
