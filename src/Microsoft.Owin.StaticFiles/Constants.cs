// -----------------------------------------------------------------------
// <copyright file="Constants.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
// -----------------------------------------------------------------------

using System.Threading.Tasks;

namespace Microsoft.Owin.StaticFiles
{
    internal static class Constants
    {
        internal const string ServerCapabilitiesKey = "server.Capabilities";
        internal const string SendFileVersionKey = "sendfile.Version";
        internal const string SendFileVersion = "1.0";

        internal const string CallCancelledKey = "owin.CallCancelled";
        internal const string RequestPathKey = "owin.RequestPath";
        internal const string RequestMethod = "owin.RequestMethod";
        internal const string ResponseHeadersKey = "owin.ResponseHeaders";
        internal const string ResponseBodyKey = "owin.ResponseBody";
        
        internal const string SendFileAsyncKey = "sendfile.SendAsync";

        internal const string ContentType = "Content-Type";
        internal const string ContentLength = "Content-Length";

        internal static readonly Task CompletedTask;

        static Constants()
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            tcs.SetResult(null);
            CompletedTask = tcs.Task;
        }
    }
}
