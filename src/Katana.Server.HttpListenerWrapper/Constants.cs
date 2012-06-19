namespace Katana.Server.HttpListenerWrapper
{
    /// <summary>
    /// Standard keys and values for use within the OWIN interfaces
    /// </summary>
    internal static class Constants
    {
        public const string VersionKey = "owin.Version";
        public const string OwinVersion = "1.0";
        public const string RequestSchemeKey = "owin.RequestScheme";
        public const string RequestMethodKey = "owin.RequestMethod";
        public const string RequestPathBaseKey = "owin.RequestPathBase";
        public const string RequestPathKey = "owin.RequestPath";
        public const string RequestQueryStringKey = "owin.RequestQueryString";
        public const string RequestHeadersKey = "owin.RequestHeaders";
        public const string RequestBodyKey = "owin.RequestBody";
        public const string HttpVersionKey = "owin.HttpVersion";
    }
}
