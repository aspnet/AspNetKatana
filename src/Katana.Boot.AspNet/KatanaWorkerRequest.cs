using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Katana.Boot.AspNet
{
    public class KatanaWorkerRequest : HttpWorkerRequest
    {
        static readonly string[] KnownResponseHeaders =
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

        readonly IDictionary<string, object> _env;
        readonly TaskCompletionSource<object> _tcsCompleted = new TaskCompletionSource<object>();
        EndOfSendNotification _endOfSendCallback;
        object _endOfSendExtraData;

        IDictionary<string, string[]> _responseHeaders;
        Stream _responseBody;

        public KatanaWorkerRequest(IDictionary<string, object> env)
        {
            _env = env;
        }

        public Task Completed
        {
            get { return _tcsCompleted.Task; }
        }

        T Get<T>(string key)
        {
            object value;
            return _env.TryGetValue(key, out value) ? (T)value : default(T);
        }

        IDictionary<string, string[]> ResponseHeaders
        {
            get { return LazyInitializer.EnsureInitialized(ref _responseHeaders, InitResponseHeaders); }
        }

        Stream ResponseBody
        {
            get { return LazyInitializer.EnsureInitialized(ref _responseBody, InitResponseBody); }
        }

        IDictionary<string, string[]> InitResponseHeaders()
        {
            return Get<IDictionary<string, string[]>>("owin.ResponseHeaders");
        }

        Stream InitResponseBody()
        {
            return Get<Stream>("owin.ResponseBody");
        }


        public override string GetUriPath()
        {
            //throw new NotImplementedException();
            return Get<string>("owin.RequestPathBase") + Get<string>("owin.RequestPath");
        }

        public override string GetQueryString()
        {
            //throw new NotImplementedException();
            return Get<string>("owin.RequestQueryString");
        }

        public override string GetRawUrl()
        {
            //throw new NotImplementedException();
            var queryString = Get<string>("owin.RequestQueryString");
            if (string.IsNullOrEmpty(queryString))
            {
                return Get<string>("owin.RequestPathBase") + Get<string>("owin.RequestPath");
            }
            return Get<string>("owin.RequestPathBase") + Get<string>("owin.RequestPath") + "?" + Get<string>("owin.RequestQueryString");
        }

        public override string GetHttpVerbName()
        {
            //throw new NotImplementedException();
            return Get<string>("owin.RequestMethod");
        }

        public override string GetHttpVersion()
        {
            //throw new NotImplementedException();
            return Get<string>("owin.RequestProtocol");
        }

        public override string GetRemoteAddress()
        {
            //throw new NotImplementedException();
            return Get<string>("server.RemoteIpAddress");
        }

        public override int GetRemotePort()
        {
            //throw new NotImplementedException();
            return int.Parse(Get<string>("server.RemotePort"));
        }

        public override string GetLocalAddress()
        {
            //throw new NotImplementedException();
            return Get<string>("server.LocalIpAddress");

        }

        public override int GetLocalPort()
        {
            //throw new NotImplementedException();
            return int.Parse(Get<string>("server.LocalPort"));
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
            return base.GetFilePath();
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
            return base.GetAppPath();
        }

        public override string GetAppPathTranslated()
        {
            return base.GetAppPathTranslated();
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
            return base.MapPath(virtualPath);
        }

        public override void SendStatus(int statusCode, string statusDescription)
        {
            //throw new NotImplementedException();
            _env["owin.ResponseStatusCode"] = statusCode;
            _env["owin.ResponseDescription"] = statusDescription;
        }



        public override void SendKnownResponseHeader(int index, string value)
        {
            //throw new NotImplementedException();
            ResponseHeaders[KnownResponseHeaders[index]] = new[] { value };
        }

        public override void SendUnknownResponseHeader(string name, string value)
        {
            //throw new NotImplementedException();
            ResponseHeaders[name] = new[] { value };
        }

        public override void SendResponseFromMemory(byte[] data, int length)
        {
            //throw new NotImplementedException();
            ResponseBody.Write(data, 0, length);
        }


        public override void SendResponseFromMemory(IntPtr data, int length)
        {
            base.SendResponseFromMemory(data, length);
        }

        public override void SendResponseFromFile(string filename, long offset, long length)
        {
            //throw new NotImplementedException();
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
            //throw new NotImplementedException();
        }

        public override void EndOfRequest()
        {
            //throw new NotImplementedException();
            if (_endOfSendCallback != null)
            {
                _endOfSendCallback(this, _endOfSendExtraData);
            }
            _tcsCompleted.SetResult(null);
        }

        public override void SetEndOfSendNotification(EndOfSendNotification callback, object extraData)
        {
            //base.SetEndOfSendNotification(callback, extraData);
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
    }
}