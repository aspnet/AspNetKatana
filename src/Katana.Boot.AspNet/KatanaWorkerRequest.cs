// <copyright file="KatanaWorkerRequest.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Katana.Boot.AspNet
{
    public class KatanaWorkerRequest : HttpWorkerRequest
    {
        private static readonly string[] KnownResponseHeaders =
            new[]
            {
                "Cache-Control",
                "Connection",
                "Date",
                "Keep-Alive",
                "Pragma",
                "Trailer",
                "Transfer-Encoding",
                "Upgrade",
                "Via",
                "Warning",
                "Allow",
                "Content-Length",
                "Content-Type",
                "Content-Encoding",
                "Content-Language",
                "Content-Location",
                "Content-MD5",
                "Content-Range",
                "Expires",
                "Last-Modified",
                "Accept-Ranges",
                "Age",
                "ETag",
                "Location",
                "Proxy-Authenticate",
                "Retry-After",
                "Server",
                "Set-Cookie",
                "Vary",
                "WWW-Authenticate",
            };

        private static readonly string[] KnownRequestHeaders =
            new[]
            {
                "Cache-Control",
                "Connection",
                "Date",
                "Keep-Alive",
                "Pragma",
                "Trailer",
                "Transfer-Encoding",
                "Upgrade",
                "Via",
                "Warning",
                "Allow",
                "Content-Length",
                "Content-Type",
                "Content-Encoding",
                "Content-Language",
                "Content-Location",
                "Content-MD5",
                "Content-Range",
                "Expires",
                "Last-Modified",
                "Accept",
                "Accept-Charset",
                "Accept-Encoding",
                "Accept-Language",
                "Authorization",
                "Cookie",
                "Expect",
                "From",
                "Host",
                "If-Match",
                "If-Modified-Since",
                "If-None-Match",
                "If-Range",
                "If-Unmodified-Since",
                "Max-Forwards",
                "Proxy-Authorization",
                "Referer",
                "Range",
                "TE",
                "User-Agent",
            };

        private readonly IDictionary<string, object> _environment;
        private readonly TaskCompletionSource<object> _tcsCompleted = new TaskCompletionSource<object>();
        private EndOfSendNotification _endOfSendCallback;
        private object _endOfSendExtraData;

        private IDictionary<string, string[]> _requestHeaders;
        private IDictionary<string, string[]> _responseHeaders;
        private Stream _requestBody;
        private Stream _responseBody;

        public KatanaWorkerRequest(IDictionary<string, object> environment)
        {
            _environment = environment;
        }

        public Task Completed
        {
            get { return _tcsCompleted.Task; }
        }

        private IDictionary<string, string[]> RequestHeaders
        {
            get { return LazyInitializer.EnsureInitialized(ref _requestHeaders, InitRequestHeaders); }
        }

        private IDictionary<string, string[]> ResponseHeaders
        {
            get { return LazyInitializer.EnsureInitialized(ref _responseHeaders, InitResponseHeaders); }
        }

        private Stream RequestBody
        {
            get { return LazyInitializer.EnsureInitialized(ref _requestBody, InitRequestBody); }
        }

        private Stream ResponseBody
        {
            get { return LazyInitializer.EnsureInitialized(ref _responseBody, InitResponseBody); }
        }

        public override string MachineConfigPath
        {
            get { return PassThrough(base.MachineConfigPath); }
        }

        public override string RootWebConfigPath
        {
            get { return PassThrough(base.RootWebConfigPath); }
        }

        public override string MachineInstallDirectory
        {
            get { return PassThrough(base.MachineInstallDirectory); }
        }

        public override Guid RequestTraceIdentifier
        {
            get { return PassThrough(base.RequestTraceIdentifier); }
        }

        private T Get<T>(string key)
        {
            object value;
            return _environment.TryGetValue(key, out value) ? (T)value : default(T);
        }

        private IDictionary<string, string[]> InitRequestHeaders()
        {
            return Get<IDictionary<string, string[]>>("owin.RequestHeaders");
        }

        private IDictionary<string, string[]> InitResponseHeaders()
        {
            return Get<IDictionary<string, string[]>>("owin.ResponseHeaders");
        }

        private Stream InitRequestBody()
        {
            return Get<Stream>("owin.RequestBody");
        }

        private Stream InitResponseBody()
        {
            return Get<Stream>("owin.ResponseBody");
        }

        public override string GetUriPath()
        {
            // throw new NotImplementedException();
            return Get<string>("owin.RequestPathBase") + Get<string>("owin.RequestPath");
        }

        public override string GetQueryString()
        {
            // throw new NotImplementedException();
            return Get<string>("owin.RequestQueryString");
        }

        public override string GetRawUrl()
        {
            // throw new NotImplementedException();
            var queryString = Get<string>("owin.RequestQueryString");
            if (string.IsNullOrEmpty(queryString))
            {
                return Get<string>("owin.RequestPathBase") + Get<string>("owin.RequestPath");
            }
            return Get<string>("owin.RequestPathBase") + Get<string>("owin.RequestPath") + "?" + Get<string>("owin.RequestQueryString");
        }

        public override string GetHttpVerbName()
        {
            // throw new NotImplementedException();
            return Get<string>("owin.RequestMethod");
        }

        public override string GetHttpVersion()
        {
            // throw new NotImplementedException();
            return Get<string>("owin.RequestProtocol");
        }

        public override string GetRemoteAddress()
        {
            // throw new NotImplementedException();
            return Get<string>("server.RemoteIpAddress");
        }

        public override int GetRemotePort()
        {
            // throw new NotImplementedException();
            return int.Parse(Get<string>("server.RemotePort"), CultureInfo.InvariantCulture);
        }

        public override string GetLocalAddress()
        {
            // throw new NotImplementedException();
            return Get<string>("server.LocalIpAddress");
        }

        public override int GetLocalPort()
        {
            // throw new NotImplementedException();
            return int.Parse(Get<string>("server.LocalPort"), CultureInfo.InvariantCulture);
        }

        public override byte[] GetQueryStringRawBytes()
        {
            return PassThrough(base.GetQueryStringRawBytes());
        }

        public override string GetRemoteName()
        {
            return PassThrough(base.GetRemoteName());
        }

        public override string GetServerName()
        {
            return PassThrough(base.GetServerName());
        }

        public override long GetConnectionID()
        {
            return PassThrough(base.GetConnectionID());
        }

        public override long GetUrlContextID()
        {
            return PassThrough(base.GetUrlContextID());
        }

        public override string GetAppPoolID()
        {
            return PassThrough(base.GetAppPoolID());
        }

        public override int GetRequestReason()
        {
            return PassThrough(base.GetRequestReason());
        }

        public override IntPtr GetUserToken()
        {
            return PassThrough(base.GetUserToken());
        }

        public override IntPtr GetVirtualPathToken()
        {
            return PassThrough(base.GetVirtualPathToken());
        }

        public override bool IsSecure()
        {
            return PassThrough(base.IsSecure());
        }

        public override string GetProtocol()
        {
            return PassThrough(base.GetProtocol());
        }

        public override string GetFilePath()
        {
            return GetUriPath();
            // return PassThrough(base.GetFilePath();
        }

        public override string GetFilePathTranslated()
        {
            return PassThrough(base.GetFilePathTranslated());
        }

        public override string GetPathInfo()
        {
            return PassThrough(base.GetPathInfo());
        }

        public override string GetAppPath()
        {
            return HttpRuntime.AppDomainAppVirtualPath;
            // return PassThrough(base.GetAppPath();
        }

        public override string GetAppPathTranslated()
        {
            return HttpRuntime.AppDomainAppPath;
            // return PassThrough(base.GetAppPathTranslated();
        }

        public override int GetPreloadedEntityBodyLength()
        {
            return PassThrough(base.GetPreloadedEntityBodyLength());
        }

        public override int GetPreloadedEntityBody(byte[] buffer, int offset)
        {
            return PassThrough(base.GetPreloadedEntityBody(buffer, offset));
        }

        public override byte[] GetPreloadedEntityBody()
        {
            return PassThrough(base.GetPreloadedEntityBody());
        }

        public override bool IsEntireEntityBodyIsPreloaded()
        {
            return PassThrough(base.IsEntireEntityBodyIsPreloaded());
        }

        public override int GetTotalEntityBodyLength()
        {
            return PassThrough(base.GetTotalEntityBodyLength());
        }

        public override int ReadEntityBody(byte[] buffer, int size)
        {
            return RequestBody.Read(buffer, 0, size);
        }

        public override int ReadEntityBody(byte[] buffer, int offset, int size)
        {
            return RequestBody.Read(buffer, offset, size);
        }

        public override string GetKnownRequestHeader(int index)
        {
            string[] value;
            if (RequestHeaders.TryGetValue(KnownRequestHeaders[index], out value)
                && value != null
                && value.Length != 0)
            {
                if (value.Length == 1)
                {
                    return value[0];
                }
                return string.Join(", ", value);
            }
            return null;
        }

        public override string GetUnknownRequestHeader(string name)
        {
            return PassThrough(base.GetUnknownRequestHeader(name));
        }

        public override string[][] GetUnknownRequestHeaders()
        {
            int count = RequestHeaders.Count;
            var headers = new string[count][];
            int index = 0;
            foreach (var kv in RequestHeaders)
            {
                if (kv.Value == null || kv.Value.Length == 0)
                {
                    headers[index] = new[] { kv.Key, string.Empty };
                }
                else if (kv.Value.Length == 1)
                {
                    headers[index] = new[] { kv.Key, kv.Value[0] };
                }
                else
                {
                    headers[index] = new[] { kv.Key, string.Join(", ", kv.Value) };
                }
                ++index;
            }
            return headers;
        }

        public override string GetServerVariable(string name)
        {
            return PassThrough(base.GetServerVariable(name));
        }

        public override long GetBytesRead()
        {
            return PassThrough(base.GetBytesRead());
        }

        public override string MapPath(string virtualPath)
        {
            string appPath = GetAppPath();
            string appPathTranslated = GetAppPathTranslated();

            if (virtualPath == null)
            {
                throw new ArgumentNullException("virtualPath");
            }

            if (virtualPath != null && virtualPath.Length == 0)
            {
                return appPathTranslated;
            }
            if (!virtualPath.StartsWith(appPath, StringComparison.Ordinal))
            {
                throw new ArgumentException("virtualPath is not rooted in the virtual directory", "virtualPath");
            }
            string text = virtualPath.Substring(appPath.Length);
            if (text.Length > 0 && text[0] == '/')
            {
                text = text.Substring(1);
            }
            if (Path.DirectorySeparatorChar != '/')
            {
                text = text.Replace('/', Path.DirectorySeparatorChar);
            }
            return Path.Combine(appPathTranslated, text);
        }

        public override void SendStatus(int statusCode, string statusDescription)
        {
            // throw new NotImplementedException();
            _environment["owin.ResponseStatusCode"] = statusCode;
            _environment["owin.ResponseDescription"] = statusDescription;
        }

        public override void SendKnownResponseHeader(int index, string value)
        {
            // throw new NotImplementedException();
            ResponseHeaders[KnownResponseHeaders[index]] = new[] { value };
        }

        public override void SendUnknownResponseHeader(string name, string value)
        {
            // throw new NotImplementedException();
            ResponseHeaders[name] = new[] { value };
        }

        public override void SendResponseFromMemory(byte[] data, int length)
        {
            // throw new NotImplementedException();
            ResponseBody.Write(data, 0, length);
        }

        public override void SendResponseFromMemory(IntPtr data, int length)
        {
            base.SendResponseFromMemory(data, length);
        }

        public override void SendResponseFromFile(string filename, long offset, long length)
        {
            // throw new NotImplementedException();
            var buffer = new byte[length];
            using (var file = new FileStream(filename, FileMode.Open))
            {
                file.Seek(offset, SeekOrigin.Begin);
                file.Read(buffer, 0, (int)length);
            }
            ResponseBody.Write(buffer, 0, (int)length);
        }

        public override void SendResponseFromFile(IntPtr handle, long offset, long length)
        {
            throw new NotImplementedException();
        }

        public override void FlushResponse(bool finalFlush)
        {
            // throw new NotImplementedException();
        }

        public override void EndOfRequest()
        {
            // throw new NotImplementedException();
            if (_endOfSendCallback != null)
            {
                _endOfSendCallback(this, _endOfSendExtraData);
            }
            _tcsCompleted.SetResult(null);
        }

        public override void SetEndOfSendNotification(EndOfSendNotification callback, object extraData)
        {
            // PassThrough(base.SetEndOfSendNotification(callback, extraData);
            _endOfSendCallback = callback;
            _endOfSendExtraData = extraData;
        }

        public override void SendCalculatedContentLength(int contentLength)
        {
            base.SendCalculatedContentLength(contentLength);
        }

        public override void SendCalculatedContentLength(long contentLength)
        {
            base.SendCalculatedContentLength(contentLength);
        }

        public override bool HeadersSent()
        {
            return PassThrough(base.HeadersSent());
        }

        public override bool IsClientConnected()
        {
            return PassThrough(base.IsClientConnected());
        }

        public override void CloseConnection()
        {
            base.CloseConnection();
        }

        public override byte[] GetClientCertificate()
        {
            return PassThrough(base.GetClientCertificate());
        }

        public override DateTime GetClientCertificateValidFrom()
        {
            return PassThrough(base.GetClientCertificateValidFrom());
        }

        public override DateTime GetClientCertificateValidUntil()
        {
            return PassThrough(base.GetClientCertificateValidUntil());
        }

        public override byte[] GetClientCertificateBinaryIssuer()
        {
            return PassThrough(base.GetClientCertificateBinaryIssuer());
        }

        public override int GetClientCertificateEncoding()
        {
            return PassThrough(base.GetClientCertificateEncoding());
        }

        public override byte[] GetClientCertificatePublicKey()
        {
            return PassThrough(base.GetClientCertificatePublicKey());
        }

        private static T PassThrough<T>(T value)
        {
            return value;
        }
    }
}
