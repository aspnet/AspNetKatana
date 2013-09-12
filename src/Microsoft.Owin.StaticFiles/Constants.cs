// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.Owin.StaticFiles
{
    internal static class Constants
    {
        internal const string ServerCapabilitiesKey = "server.Capabilities";
        internal const string SendFileVersionKey = "sendfile.Version";
        internal const string SendFileVersion = "1.0";

        internal const string CallCancelledKey = "owin.CallCancelled";
        internal const string RequestPathBaseKey = "owin.RequestPathBase";
        internal const string RequestPathKey = "owin.RequestPath";
        internal const string RequestHeadersKey = "owin.RequestHeaders";
        internal const string RequestMethod = "owin.RequestMethod";
        internal const string ResponseHeadersKey = "owin.ResponseHeaders";
        internal const string ResponseBodyKey = "owin.ResponseBody";
        internal const string ResponseStatusCodeKey = "owin.ResponseStatusCode";

        internal const string SendFileAsyncKey = "sendfile.SendAsync";

        internal const string Accept = "Accept";
        internal const string ContentType = "Content-Type";
        internal const string ContentLength = "Content-Length";
        internal const string Location = "Location";

        internal const string ApplicationJson = "application/json";
        internal const string TextPlain = "text/plain";
        internal const string TextHtml = "text/html";
        internal const string AnyType = "*/*";

        internal const int Status200Ok = 200;
        internal const int Status304NotModified = 304;
        internal const int Status412PreconditionFailed = 412;

        internal static readonly Task CompletedTask = CreateCompletedTask();

        private static Task CreateCompletedTask()
        {
            var tcs = new TaskCompletionSource<object>();
            tcs.SetResult(null);
            return tcs.Task;
        }
    }
}
