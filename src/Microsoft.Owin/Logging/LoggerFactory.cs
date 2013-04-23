namespace Microsoft.Owin.Logging
{
    public static class LoggerFactory
    {
        static LoggerFactory()
        {
            Default = new DiagnosticsLoggerFactory();
        }

        public static ILoggerFactory Default { get; set; }
    }
}