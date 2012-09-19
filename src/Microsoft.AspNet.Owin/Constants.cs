//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.AspNet.Owin
{
    /// <summary>
    /// Standard keys and values for use within the OWIN interfaces
    /// </summary>
    internal static class Constants
    {
        public const string ServerNameKey = "server.Name";
        public static readonly string ServerName = "ASP.NET 4.0, Microsoft.AspNet.Owin " + typeof(Constants).Assembly.GetName().Version.ToString();
        public const string ServerVersionKey = "msaspnet.AdapterVersion";
        public static readonly string ServerVersion = typeof(Constants).Assembly.GetName().Version.ToString();

        public const string SendFileVersionKey = "sendfile.Version";
        public const string SendFileVersion = "1.0";
        public const string SendFileSupportKey = "sendfile.Support";
        public const string SendFileSupport = "SendFileFunc";
        public const string SendFileFuncKey = "sendfile.Func";
    }
}
