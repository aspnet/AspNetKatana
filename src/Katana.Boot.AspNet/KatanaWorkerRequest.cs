// <copyright file="KatanaWorkerRequest.cs" company="Katana contributors">
//   Copyright 2011-2013 Katana contributors
// </copyright>
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

        private readonly IDictionary<string, object> _environment;
        private readonly TaskCompletionSource<object> _tcsCompleted = new TaskCompletionSource<object>();
        private EndOfSendNotification _endOfSendCallback;
        private object _endOfSendExtraData;

        private IDictionary<string, string[]> _responseHeaders;
        private Stream _responseBody;

        public KatanaWorkerRequest(IDictionary<string, object> environment)
        {
            _environment = environment;
        }

        public Task Completed
        {
            get { return _tcsCompleted.Task; }
        }

        private IDictionary<string, string[]> ResponseHeaders
        {
            get { return LazyInitializer.EnsureInitialized(ref _responseHeaders, InitResponseHeaders); }
        }

        private Stream ResponseBody
        {
            get { return LazyInitializer.EnsureInitialized(ref _responseBody, InitResponseBody); }
        }

        public override string MachineConfigPath
        {
            get { return base.MachineConfigPath; }
        }

        public override string RootWebConfigPath
        {
            get { return base.RootWebConfigPath; }
        }

        public override string MachineInstallDirectory
        {
            get { return base.MachineInstallDirectory; }
        }

        public override Guid RequestTraceIdentifier
        {
            get { return base.RequestTraceIdentifier; }
        }

        private T Get<T>(string key)
        {
            object value;
            return _environment.TryGetValue(key, out value) ? (T)value : default(T);
        }

        private IDictionary<string, string[]> InitResponseHeaders()
        {
            return Get<IDictionary<string, string[]>>("owin.ResponseHeaders");
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
            return base.GetQueryStringRawBytes();
        }

        public override string GetRemoteName()
        {
            return base.GetRemoteName();
        }

        public override string GetServerName()
        {
            return base.GetServerName();
        }

        public override long GetConnectionID()
        {
            return base.GetConnectionID();
        }

        public override long GetUrlContextID()
        {
            return base.GetUrlContextID();
        }

        public override string GetAppPoolID()
        {
            return base.GetAppPoolID();
        }

        public override int GetRequestReason()
        {
            return base.GetRequestReason();
        }

        public override IntPtr GetUserToken()
        {
            return base.GetUserToken();
        }

        public override IntPtr GetVirtualPathToken()
        {
            return base.GetVirtualPathToken();
        }

        public override bool IsSecure()
        {
            return base.IsSecure();
        }

        public override string GetProtocol()
        {
            return base.GetProtocol();
        }

        public override string GetFilePath()
        {
            return GetUriPath();
            // return base.GetFilePath();
        }

        public override string GetFilePathTranslated()
        {
            return base.GetFilePathTranslated();
        }

        public override string GetPathInfo()
        {
            return base.GetPathInfo();
        }

        public override string GetAppPath()
        {
            return HttpRuntime.AppDomainAppVirtualPath;
            // return base.GetAppPath();
        }

        public override string GetAppPathTranslated()
        {
            return HttpRuntime.AppDomainAppPath;
            // return base.GetAppPathTranslated();
        }

        public override int GetPreloadedEntityBodyLength()
        {
            return base.GetPreloadedEntityBodyLength();
        }

        public override int GetPreloadedEntityBody(byte[] buffer, int offset)
        {
            return base.GetPreloadedEntityBody(buffer, offset);
        }

        public override byte[] GetPreloadedEntityBody()
        {
            return base.GetPreloadedEntityBody();
        }

        public override bool IsEntireEntityBodyIsPreloaded()
        {
            return base.IsEntireEntityBodyIsPreloaded();
        }

        public override int GetTotalEntityBodyLength()
        {
            return base.GetTotalEntityBodyLength();
        }

        public override int ReadEntityBody(byte[] buffer, int size)
        {
            return base.ReadEntityBody(buffer, size);
        }

        public override int ReadEntityBody(byte[] buffer, int offset, int size)
        {
            return base.ReadEntityBody(buffer, offset, size);
        }

        public override string GetKnownRequestHeader(int index)
        {
            return base.GetKnownRequestHeader(index);
        }

        public override string GetUnknownRequestHeader(string name)
        {
            return base.GetUnknownRequestHeader(name);
        }

        public override string[][] GetUnknownRequestHeaders()
        {
            return base.GetUnknownRequestHeaders();
        }

        public override string GetServerVariable(string name)
        {
            return base.GetServerVariable(name);
        }

        public override long GetBytesRead()
        {
            return base.GetBytesRead();
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
            // base.SetEndOfSendNotification(callback, extraData);
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
            return base.HeadersSent();
        }

        public override bool IsClientConnected()
        {
            return base.IsClientConnected();
        }

        public override void CloseConnection()
        {
            base.CloseConnection();
        }

        public override byte[] GetClientCertificate()
        {
            return base.GetClientCertificate();
        }

        public override DateTime GetClientCertificateValidFrom()
        {
            return base.GetClientCertificateValidFrom();
        }

        public override DateTime GetClientCertificateValidUntil()
        {
            return base.GetClientCertificateValidUntil();
        }

        public override byte[] GetClientCertificateBinaryIssuer()
        {
            return base.GetClientCertificateBinaryIssuer();
        }

        public override int GetClientCertificateEncoding()
        {
            return base.GetClientCertificateEncoding();
        }

        public override byte[] GetClientCertificatePublicKey()
        {
            return base.GetClientCertificatePublicKey();
        }
    }
}
