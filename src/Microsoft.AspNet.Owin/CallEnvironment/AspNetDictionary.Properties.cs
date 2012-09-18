
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;

namespace Microsoft.AspNet.Owin.CallEnvironment
{
	public partial class AspNetDictionary
	{
        UInt32 _flag0;

        string _OwinVersion;
        public string OwinVersion 
        {
            get {return _OwinVersion;}
            set {_flag0 |= 0x1u; _OwinVersion = value;} 
        }
        CancellationToken _CallCancelled;
        public CancellationToken CallCancelled 
        {
            get {return _CallCancelled;}
            set {_flag0 |= 0x2u; _CallCancelled = value;} 
        }
        string _RequestProtocol;
        public string RequestProtocol 
        {
            get {return _RequestProtocol;}
            set {_flag0 |= 0x4u; _RequestProtocol = value;} 
        }
        string _RequestMethod;
        public string RequestMethod 
        {
            get {return _RequestMethod;}
            set {_flag0 |= 0x8u; _RequestMethod = value;} 
        }
        string _RequestScheme;
        public string RequestScheme 
        {
            get {return _RequestScheme;}
            set {_flag0 |= 0x10u; _RequestScheme = value;} 
        }
        string _RequestPathBase;
        public string RequestPathBase 
        {
            get {return _RequestPathBase;}
            set {_flag0 |= 0x20u; _RequestPathBase = value;} 
        }
        string _RequestPath;
        public string RequestPath 
        {
            get {return _RequestPath;}
            set {_flag0 |= 0x40u; _RequestPath = value;} 
        }
        string _RequestQueryString;
        public string RequestQueryString 
        {
            get {return _RequestQueryString;}
            set {_flag0 |= 0x80u; _RequestQueryString = value;} 
        }
        IDictionary<string,string[]> _RequestHeaders;
        public IDictionary<string,string[]> RequestHeaders 
        {
            get {return _RequestHeaders;}
            set {_flag0 |= 0x100u; _RequestHeaders = value;} 
        }
        Stream _RequestBody;
        public Stream RequestBody 
        {
            get {return _RequestBody;}
            set {_flag0 |= 0x200u; _RequestBody = value;} 
        }
        int _ResponseStatusCode;
        public int ResponseStatusCode 
        {
            get {return _ResponseStatusCode;}
            set {_flag0 |= 0x400u; _ResponseStatusCode = value;} 
        }
        string _ResponseReasonPhrase;
        public string ResponseReasonPhrase 
        {
            get {return _ResponseReasonPhrase;}
            set {_flag0 |= 0x800u; _ResponseReasonPhrase = value;} 
        }
        IDictionary<string,string[]> _ResponseHeaders;
        public IDictionary<string,string[]> ResponseHeaders 
        {
            get {return _ResponseHeaders;}
            set {_flag0 |= 0x1000u; _ResponseHeaders = value;} 
        }
        Stream _ResponseBody;
        public Stream ResponseBody 
        {
            get {return _ResponseBody;}
            set {_flag0 |= 0x2000u; _ResponseBody = value;} 
        }
        TextWriter _HostTraceOutput;
        public TextWriter HostTraceOutput 
        {
            get {return _HostTraceOutput;}
            set {_flag0 |= 0x4000u; _HostTraceOutput = value;} 
        }
        Action _ServerDisableResponseBuffering;
        public Action ServerDisableResponseBuffering 
        {
            get {return _ServerDisableResponseBuffering;}
            set {_flag0 |= 0x8000u; _ServerDisableResponseBuffering = value;} 
        }
        System.Security.Principal.IPrincipal _ServerUser;
        public System.Security.Principal.IPrincipal ServerUser 
        {
            get {return _ServerUser;}
            set {_flag0 |= 0x10000u; _ServerUser = value;} 
        }
        string _ServerRemoteIpAddress;
        public string ServerRemoteIpAddress 
        {
            get {return _ServerRemoteIpAddress;}
            set {_flag0 |= 0x20000u; _ServerRemoteIpAddress = value;} 
        }
        string _ServerRemotePort;
        public string ServerRemotePort 
        {
            get {return _ServerRemotePort;}
            set {_flag0 |= 0x40000u; _ServerRemotePort = value;} 
        }
        string _ServerLocalIpAddress;
        public string ServerLocalIpAddress 
        {
            get {return _ServerLocalIpAddress;}
            set {_flag0 |= 0x80000u; _ServerLocalIpAddress = value;} 
        }
        string _ServerLocalPort;
        public string ServerLocalPort 
        {
            get {return _ServerLocalPort;}
            set {_flag0 |= 0x100000u; _ServerLocalPort = value;} 
        }
        bool _ServerIsLocal;
        public bool ServerIsLocal 
        {
            get {return _ServerIsLocal;}
            set {_flag0 |= 0x200000u; _ServerIsLocal = value;} 
        }
        object _WebSocketSupport;
        public object WebSocketSupport 
        {
            get {return _WebSocketSupport;}
            set {_flag0 |= 0x400000u; _WebSocketSupport = value;} 
        }
        object _WebSocketAccept;
        public object WebSocketAccept 
        {
            get {return _WebSocketAccept;}
            set {_flag0 |= 0x800000u; _WebSocketAccept = value;} 
        }
        string _SendFileVersion;
        public string SendFileVersion 
        {
            get {return _SendFileVersion;}
            set {_flag0 |= 0x1000000u; _SendFileVersion = value;} 
        }
        string _SendFileSupport;
        public string SendFileSupport 
        {
            get {return _SendFileSupport;}
            set {_flag0 |= 0x2000000u; _SendFileSupport = value;} 
        }
        Func<string,long,long?,Task> _SendFileFunc;
        public Func<string,long,long?,Task> SendFileFunc 
        {
            get {return _SendFileFunc;}
            set {_flag0 |= 0x4000000u; _SendFileFunc = value;} 
        }
        string _SendFileConcurrency;
        public string SendFileConcurrency 
        {
            get {return _SendFileConcurrency;}
            set {_flag0 |= 0x8000000u; _SendFileConcurrency = value;} 
        }
        string _ServerName;
        public string ServerName 
        {
            get {return _ServerName;}
            set {_flag0 |= 0x10000000u; _ServerName = value;} 
        }
        string _ServerVersion;
        public string ServerVersion 
        {
            get {return _ServerVersion;}
            set {_flag0 |= 0x20000000u; _ServerVersion = value;} 
        }
        RequestContext _RequestContext;
        public RequestContext RequestContext 
        {
            get {return _RequestContext;}
            set {_flag0 |= 0x40000000u; _RequestContext = value;} 
        }
        HttpContextBase _HttpContextBase;
        public HttpContextBase HttpContextBase 
        {
            get {return _HttpContextBase;}
            set {_flag0 |= 0x80000000u; _HttpContextBase = value;} 
        }

        bool PropertiesContainsKey(string key)
        {
            switch(key.Length)
            {
                case 12:
                    if (((_flag0 & 0x1u) != 0) && string.Equals(key, "owin.Version", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                   break;
                case 18:
                    if (((_flag0 & 0x2u) != 0) && string.Equals(key, "owin.CallCancelled", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                    if (((_flag0 & 0x8u) != 0) && string.Equals(key, "owin.RequestMethod", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                    if (((_flag0 & 0x10u) != 0) && string.Equals(key, "owin.RequestScheme", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                   break;
                case 20:
                    if (((_flag0 & 0x4u) != 0) && string.Equals(key, "owin.RequestProtocol", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                    if (((_flag0 & 0x20u) != 0) && string.Equals(key, "owin.RequestPathBase", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                    if (((_flag0 & 0x1000u) != 0) && string.Equals(key, "owin.ResponseHeaders", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                    if (((_flag0 & 0x8000000u) != 0) && string.Equals(key, "sendfile.Concurrency", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                   break;
                case 16:
                    if (((_flag0 & 0x40u) != 0) && string.Equals(key, "owin.RequestPath", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                    if (((_flag0 & 0x200u) != 0) && string.Equals(key, "owin.RequestBody", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                    if (((_flag0 & 0x4000u) != 0) && string.Equals(key, "host.TraceOutput", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                    if (((_flag0 & 0x100000u) != 0) && string.Equals(key, "server.LocalPort", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                    if (((_flag0 & 0x800000u) != 0) && string.Equals(key, "websocket.Accept", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                    if (((_flag0 & 0x1000000u) != 0) && string.Equals(key, "sendfile.Version", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                    if (((_flag0 & 0x2000000u) != 0) && string.Equals(key, "sendfile.Support", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                    if (((_flag0 & 0x20000000u) != 0) && string.Equals(key, "msaspnet.Version", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                   break;
                case 23:
                    if (((_flag0 & 0x80u) != 0) && string.Equals(key, "owin.RequestQueryString", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                    if (((_flag0 & 0x400u) != 0) && string.Equals(key, "owin.ResponseStatusCode", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                   break;
                case 19:
                    if (((_flag0 & 0x100u) != 0) && string.Equals(key, "owin.RequestHeaders", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                   break;
                case 25:
                    if (((_flag0 & 0x800u) != 0) && string.Equals(key, "owin.ResponseReasonPhrase", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                   break;
                case 17:
                    if (((_flag0 & 0x2000u) != 0) && string.Equals(key, "owin.ResponseBody", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                    if (((_flag0 & 0x40000u) != 0) && string.Equals(key, "server.RemotePort", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                    if (((_flag0 & 0x400000u) != 0) && string.Equals(key, "websocket.Support", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                   break;
                case 31:
                    if (((_flag0 & 0x8000u) != 0) && string.Equals(key, "server.DisableResponseBuffering", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                   break;
                case 11:
                    if (((_flag0 & 0x10000u) != 0) && string.Equals(key, "server.User", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                    if (((_flag0 & 0x10000000u) != 0) && string.Equals(key, "server.Name", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                   break;
                case 22:
                    if (((_flag0 & 0x20000u) != 0) && string.Equals(key, "server.RemoteIpAddress", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                   break;
                case 21:
                    if (((_flag0 & 0x80000u) != 0) && string.Equals(key, "server.LocalIpAddress", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                   break;
                case 14:
                    if (((_flag0 & 0x200000u) != 0) && string.Equals(key, "server.IsLocal", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                   break;
                case 13:
                    if (((_flag0 & 0x4000000u) != 0) && string.Equals(key, "sendfile.Func", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                   break;
                case 33:
                    if (((_flag0 & 0x40000000u) != 0) && string.Equals(key, "System.Web.Routing.RequestContext", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                   break;
                case 26:
                    if (((_flag0 & 0x80000000u) != 0) && string.Equals(key, "System.Web.HttpContextBase", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                   break;
            }
            return false;
        }

        bool PropertiesTryGetValue(string key, out object value)
        {
            switch(key.Length)
            {
                case 12:
                    if (((_flag0 & 0x1u) != 0) && string.Equals(key, "owin.Version", StringComparison.Ordinal)) 
                    {
                        value = OwinVersion;
                        return true;
                    }
                   break;
                case 18:
                    if (((_flag0 & 0x2u) != 0) && string.Equals(key, "owin.CallCancelled", StringComparison.Ordinal)) 
                    {
                        value = CallCancelled;
                        return true;
                    }
                    if (((_flag0 & 0x8u) != 0) && string.Equals(key, "owin.RequestMethod", StringComparison.Ordinal)) 
                    {
                        value = RequestMethod;
                        return true;
                    }
                    if (((_flag0 & 0x10u) != 0) && string.Equals(key, "owin.RequestScheme", StringComparison.Ordinal)) 
                    {
                        value = RequestScheme;
                        return true;
                    }
                   break;
                case 20:
                    if (((_flag0 & 0x4u) != 0) && string.Equals(key, "owin.RequestProtocol", StringComparison.Ordinal)) 
                    {
                        value = RequestProtocol;
                        return true;
                    }
                    if (((_flag0 & 0x20u) != 0) && string.Equals(key, "owin.RequestPathBase", StringComparison.Ordinal)) 
                    {
                        value = RequestPathBase;
                        return true;
                    }
                    if (((_flag0 & 0x1000u) != 0) && string.Equals(key, "owin.ResponseHeaders", StringComparison.Ordinal)) 
                    {
                        value = ResponseHeaders;
                        return true;
                    }
                    if (((_flag0 & 0x8000000u) != 0) && string.Equals(key, "sendfile.Concurrency", StringComparison.Ordinal)) 
                    {
                        value = SendFileConcurrency;
                        return true;
                    }
                   break;
                case 16:
                    if (((_flag0 & 0x40u) != 0) && string.Equals(key, "owin.RequestPath", StringComparison.Ordinal)) 
                    {
                        value = RequestPath;
                        return true;
                    }
                    if (((_flag0 & 0x200u) != 0) && string.Equals(key, "owin.RequestBody", StringComparison.Ordinal)) 
                    {
                        value = RequestBody;
                        return true;
                    }
                    if (((_flag0 & 0x4000u) != 0) && string.Equals(key, "host.TraceOutput", StringComparison.Ordinal)) 
                    {
                        value = HostTraceOutput;
                        return true;
                    }
                    if (((_flag0 & 0x100000u) != 0) && string.Equals(key, "server.LocalPort", StringComparison.Ordinal)) 
                    {
                        value = ServerLocalPort;
                        return true;
                    }
                    if (((_flag0 & 0x800000u) != 0) && string.Equals(key, "websocket.Accept", StringComparison.Ordinal)) 
                    {
                        value = WebSocketAccept;
                        return true;
                    }
                    if (((_flag0 & 0x1000000u) != 0) && string.Equals(key, "sendfile.Version", StringComparison.Ordinal)) 
                    {
                        value = SendFileVersion;
                        return true;
                    }
                    if (((_flag0 & 0x2000000u) != 0) && string.Equals(key, "sendfile.Support", StringComparison.Ordinal)) 
                    {
                        value = SendFileSupport;
                        return true;
                    }
                    if (((_flag0 & 0x20000000u) != 0) && string.Equals(key, "msaspnet.Version", StringComparison.Ordinal)) 
                    {
                        value = ServerVersion;
                        return true;
                    }
                   break;
                case 23:
                    if (((_flag0 & 0x80u) != 0) && string.Equals(key, "owin.RequestQueryString", StringComparison.Ordinal)) 
                    {
                        value = RequestQueryString;
                        return true;
                    }
                    if (((_flag0 & 0x400u) != 0) && string.Equals(key, "owin.ResponseStatusCode", StringComparison.Ordinal)) 
                    {
                        value = ResponseStatusCode;
                        return true;
                    }
                   break;
                case 19:
                    if (((_flag0 & 0x100u) != 0) && string.Equals(key, "owin.RequestHeaders", StringComparison.Ordinal)) 
                    {
                        value = RequestHeaders;
                        return true;
                    }
                   break;
                case 25:
                    if (((_flag0 & 0x800u) != 0) && string.Equals(key, "owin.ResponseReasonPhrase", StringComparison.Ordinal)) 
                    {
                        value = ResponseReasonPhrase;
                        return true;
                    }
                   break;
                case 17:
                    if (((_flag0 & 0x2000u) != 0) && string.Equals(key, "owin.ResponseBody", StringComparison.Ordinal)) 
                    {
                        value = ResponseBody;
                        return true;
                    }
                    if (((_flag0 & 0x40000u) != 0) && string.Equals(key, "server.RemotePort", StringComparison.Ordinal)) 
                    {
                        value = ServerRemotePort;
                        return true;
                    }
                    if (((_flag0 & 0x400000u) != 0) && string.Equals(key, "websocket.Support", StringComparison.Ordinal)) 
                    {
                        value = WebSocketSupport;
                        return true;
                    }
                   break;
                case 31:
                    if (((_flag0 & 0x8000u) != 0) && string.Equals(key, "server.DisableResponseBuffering", StringComparison.Ordinal)) 
                    {
                        value = ServerDisableResponseBuffering;
                        return true;
                    }
                   break;
                case 11:
                    if (((_flag0 & 0x10000u) != 0) && string.Equals(key, "server.User", StringComparison.Ordinal)) 
                    {
                        value = ServerUser;
                        return true;
                    }
                    if (((_flag0 & 0x10000000u) != 0) && string.Equals(key, "server.Name", StringComparison.Ordinal)) 
                    {
                        value = ServerName;
                        return true;
                    }
                   break;
                case 22:
                    if (((_flag0 & 0x20000u) != 0) && string.Equals(key, "server.RemoteIpAddress", StringComparison.Ordinal)) 
                    {
                        value = ServerRemoteIpAddress;
                        return true;
                    }
                   break;
                case 21:
                    if (((_flag0 & 0x80000u) != 0) && string.Equals(key, "server.LocalIpAddress", StringComparison.Ordinal)) 
                    {
                        value = ServerLocalIpAddress;
                        return true;
                    }
                   break;
                case 14:
                    if (((_flag0 & 0x200000u) != 0) && string.Equals(key, "server.IsLocal", StringComparison.Ordinal)) 
                    {
                        value = ServerIsLocal;
                        return true;
                    }
                   break;
                case 13:
                    if (((_flag0 & 0x4000000u) != 0) && string.Equals(key, "sendfile.Func", StringComparison.Ordinal)) 
                    {
                        value = SendFileFunc;
                        return true;
                    }
                   break;
                case 33:
                    if (((_flag0 & 0x40000000u) != 0) && string.Equals(key, "System.Web.Routing.RequestContext", StringComparison.Ordinal)) 
                    {
                        value = RequestContext;
                        return true;
                    }
                   break;
                case 26:
                    if (((_flag0 & 0x80000000u) != 0) && string.Equals(key, "System.Web.HttpContextBase", StringComparison.Ordinal)) 
                    {
                        value = HttpContextBase;
                        return true;
                    }
                   break;
            }
            value = null;
            return false;
        }

        bool PropertiesTrySetValue(string key, object value)
        {
            switch(key.Length)
            {
                case 12:
                    if (string.Equals(key, "owin.Version", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x1u;
                        OwinVersion = (string)value;
                        return true;
                    }
                   break;
                case 18:
                    if (string.Equals(key, "owin.CallCancelled", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x2u;
                        CallCancelled = (CancellationToken)value;
                        return true;
                    }
                    if (string.Equals(key, "owin.RequestMethod", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x8u;
                        RequestMethod = (string)value;
                        return true;
                    }
                    if (string.Equals(key, "owin.RequestScheme", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x10u;
                        RequestScheme = (string)value;
                        return true;
                    }
                   break;
                case 20:
                    if (string.Equals(key, "owin.RequestProtocol", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x4u;
                        RequestProtocol = (string)value;
                        return true;
                    }
                    if (string.Equals(key, "owin.RequestPathBase", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x20u;
                        RequestPathBase = (string)value;
                        return true;
                    }
                    if (string.Equals(key, "owin.ResponseHeaders", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x1000u;
                        ResponseHeaders = (IDictionary<string,string[]>)value;
                        return true;
                    }
                    if (string.Equals(key, "sendfile.Concurrency", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x8000000u;
                        SendFileConcurrency = (string)value;
                        return true;
                    }
                   break;
                case 16:
                    if (string.Equals(key, "owin.RequestPath", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x40u;
                        RequestPath = (string)value;
                        return true;
                    }
                    if (string.Equals(key, "owin.RequestBody", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x200u;
                        RequestBody = (Stream)value;
                        return true;
                    }
                    if (string.Equals(key, "host.TraceOutput", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x4000u;
                        HostTraceOutput = (TextWriter)value;
                        return true;
                    }
                    if (string.Equals(key, "server.LocalPort", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x100000u;
                        ServerLocalPort = (string)value;
                        return true;
                    }
                    if (string.Equals(key, "websocket.Accept", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x800000u;
                        WebSocketAccept = (object)value;
                        return true;
                    }
                    if (string.Equals(key, "sendfile.Version", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x1000000u;
                        SendFileVersion = (string)value;
                        return true;
                    }
                    if (string.Equals(key, "sendfile.Support", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x2000000u;
                        SendFileSupport = (string)value;
                        return true;
                    }
                    if (string.Equals(key, "msaspnet.Version", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x20000000u;
                        ServerVersion = (string)value;
                        return true;
                    }
                   break;
                case 23:
                    if (string.Equals(key, "owin.RequestQueryString", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x80u;
                        RequestQueryString = (string)value;
                        return true;
                    }
                    if (string.Equals(key, "owin.ResponseStatusCode", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x400u;
                        ResponseStatusCode = (int)value;
                        return true;
                    }
                   break;
                case 19:
                    if (string.Equals(key, "owin.RequestHeaders", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x100u;
                        RequestHeaders = (IDictionary<string,string[]>)value;
                        return true;
                    }
                   break;
                case 25:
                    if (string.Equals(key, "owin.ResponseReasonPhrase", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x800u;
                        ResponseReasonPhrase = (string)value;
                        return true;
                    }
                   break;
                case 17:
                    if (string.Equals(key, "owin.ResponseBody", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x2000u;
                        ResponseBody = (Stream)value;
                        return true;
                    }
                    if (string.Equals(key, "server.RemotePort", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x40000u;
                        ServerRemotePort = (string)value;
                        return true;
                    }
                    if (string.Equals(key, "websocket.Support", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x400000u;
                        WebSocketSupport = (object)value;
                        return true;
                    }
                   break;
                case 31:
                    if (string.Equals(key, "server.DisableResponseBuffering", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x8000u;
                        ServerDisableResponseBuffering = (Action)value;
                        return true;
                    }
                   break;
                case 11:
                    if (string.Equals(key, "server.User", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x10000u;
                        ServerUser = (System.Security.Principal.IPrincipal)value;
                        return true;
                    }
                    if (string.Equals(key, "server.Name", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x10000000u;
                        ServerName = (string)value;
                        return true;
                    }
                   break;
                case 22:
                    if (string.Equals(key, "server.RemoteIpAddress", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x20000u;
                        ServerRemoteIpAddress = (string)value;
                        return true;
                    }
                   break;
                case 21:
                    if (string.Equals(key, "server.LocalIpAddress", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x80000u;
                        ServerLocalIpAddress = (string)value;
                        return true;
                    }
                   break;
                case 14:
                    if (string.Equals(key, "server.IsLocal", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x200000u;
                        ServerIsLocal = (bool)value;
                        return true;
                    }
                   break;
                case 13:
                    if (string.Equals(key, "sendfile.Func", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x4000000u;
                        SendFileFunc = (Func<string,long,long?,Task>)value;
                        return true;
                    }
                   break;
                case 33:
                    if (string.Equals(key, "System.Web.Routing.RequestContext", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x40000000u;
                        RequestContext = (RequestContext)value;
                        return true;
                    }
                   break;
                case 26:
                    if (string.Equals(key, "System.Web.HttpContextBase", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x80000000u;
                        HttpContextBase = (HttpContextBase)value;
                        return true;
                    }
                   break;
            }
            return false;
        }

        bool PropertiesTryRemove(string key)
        {
            switch(key.Length)
            {
                case 12:
                    if (((_flag0 & 0x1u) != 0) && string.Equals(key, "owin.Version", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x1u;
                        OwinVersion = default(string);
                        return true;
                    }
                   break;
                case 18:
                    if (((_flag0 & 0x2u) != 0) && string.Equals(key, "owin.CallCancelled", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x2u;
                        CallCancelled = default(CancellationToken);
                        return true;
                    }
                    if (((_flag0 & 0x8u) != 0) && string.Equals(key, "owin.RequestMethod", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x8u;
                        RequestMethod = default(string);
                        return true;
                    }
                    if (((_flag0 & 0x10u) != 0) && string.Equals(key, "owin.RequestScheme", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x10u;
                        RequestScheme = default(string);
                        return true;
                    }
                   break;
                case 20:
                    if (((_flag0 & 0x4u) != 0) && string.Equals(key, "owin.RequestProtocol", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x4u;
                        RequestProtocol = default(string);
                        return true;
                    }
                    if (((_flag0 & 0x20u) != 0) && string.Equals(key, "owin.RequestPathBase", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x20u;
                        RequestPathBase = default(string);
                        return true;
                    }
                    if (((_flag0 & 0x1000u) != 0) && string.Equals(key, "owin.ResponseHeaders", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x1000u;
                        ResponseHeaders = default(IDictionary<string,string[]>);
                        return true;
                    }
                    if (((_flag0 & 0x8000000u) != 0) && string.Equals(key, "sendfile.Concurrency", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x8000000u;
                        SendFileConcurrency = default(string);
                        return true;
                    }
                   break;
                case 16:
                    if (((_flag0 & 0x40u) != 0) && string.Equals(key, "owin.RequestPath", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x40u;
                        RequestPath = default(string);
                        return true;
                    }
                    if (((_flag0 & 0x200u) != 0) && string.Equals(key, "owin.RequestBody", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x200u;
                        RequestBody = default(Stream);
                        return true;
                    }
                    if (((_flag0 & 0x4000u) != 0) && string.Equals(key, "host.TraceOutput", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x4000u;
                        HostTraceOutput = default(TextWriter);
                        return true;
                    }
                    if (((_flag0 & 0x100000u) != 0) && string.Equals(key, "server.LocalPort", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x100000u;
                        ServerLocalPort = default(string);
                        return true;
                    }
                    if (((_flag0 & 0x800000u) != 0) && string.Equals(key, "websocket.Accept", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x800000u;
                        WebSocketAccept = default(object);
                        return true;
                    }
                    if (((_flag0 & 0x1000000u) != 0) && string.Equals(key, "sendfile.Version", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x1000000u;
                        SendFileVersion = default(string);
                        return true;
                    }
                    if (((_flag0 & 0x2000000u) != 0) && string.Equals(key, "sendfile.Support", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x2000000u;
                        SendFileSupport = default(string);
                        return true;
                    }
                    if (((_flag0 & 0x20000000u) != 0) && string.Equals(key, "msaspnet.Version", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x20000000u;
                        ServerVersion = default(string);
                        return true;
                    }
                   break;
                case 23:
                    if (((_flag0 & 0x80u) != 0) && string.Equals(key, "owin.RequestQueryString", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x80u;
                        RequestQueryString = default(string);
                        return true;
                    }
                    if (((_flag0 & 0x400u) != 0) && string.Equals(key, "owin.ResponseStatusCode", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x400u;
                        ResponseStatusCode = default(int);
                        return true;
                    }
                   break;
                case 19:
                    if (((_flag0 & 0x100u) != 0) && string.Equals(key, "owin.RequestHeaders", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x100u;
                        RequestHeaders = default(IDictionary<string,string[]>);
                        return true;
                    }
                   break;
                case 25:
                    if (((_flag0 & 0x800u) != 0) && string.Equals(key, "owin.ResponseReasonPhrase", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x800u;
                        ResponseReasonPhrase = default(string);
                        return true;
                    }
                   break;
                case 17:
                    if (((_flag0 & 0x2000u) != 0) && string.Equals(key, "owin.ResponseBody", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x2000u;
                        ResponseBody = default(Stream);
                        return true;
                    }
                    if (((_flag0 & 0x40000u) != 0) && string.Equals(key, "server.RemotePort", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x40000u;
                        ServerRemotePort = default(string);
                        return true;
                    }
                    if (((_flag0 & 0x400000u) != 0) && string.Equals(key, "websocket.Support", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x400000u;
                        WebSocketSupport = default(object);
                        return true;
                    }
                   break;
                case 31:
                    if (((_flag0 & 0x8000u) != 0) && string.Equals(key, "server.DisableResponseBuffering", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x8000u;
                        ServerDisableResponseBuffering = default(Action);
                        return true;
                    }
                   break;
                case 11:
                    if (((_flag0 & 0x10000u) != 0) && string.Equals(key, "server.User", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x10000u;
                        ServerUser = default(System.Security.Principal.IPrincipal);
                        return true;
                    }
                    if (((_flag0 & 0x10000000u) != 0) && string.Equals(key, "server.Name", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x10000000u;
                        ServerName = default(string);
                        return true;
                    }
                   break;
                case 22:
                    if (((_flag0 & 0x20000u) != 0) && string.Equals(key, "server.RemoteIpAddress", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x20000u;
                        ServerRemoteIpAddress = default(string);
                        return true;
                    }
                   break;
                case 21:
                    if (((_flag0 & 0x80000u) != 0) && string.Equals(key, "server.LocalIpAddress", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x80000u;
                        ServerLocalIpAddress = default(string);
                        return true;
                    }
                   break;
                case 14:
                    if (((_flag0 & 0x200000u) != 0) && string.Equals(key, "server.IsLocal", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x200000u;
                        ServerIsLocal = default(bool);
                        return true;
                    }
                   break;
                case 13:
                    if (((_flag0 & 0x4000000u) != 0) && string.Equals(key, "sendfile.Func", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x4000000u;
                        SendFileFunc = default(Func<string,long,long?,Task>);
                        return true;
                    }
                   break;
                case 33:
                    if (((_flag0 & 0x40000000u) != 0) && string.Equals(key, "System.Web.Routing.RequestContext", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x40000000u;
                        RequestContext = default(RequestContext);
                        return true;
                    }
                   break;
                case 26:
                    if (((_flag0 & 0x80000000u) != 0) && string.Equals(key, "System.Web.HttpContextBase", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x80000000u;
                        HttpContextBase = default(HttpContextBase);
                        return true;
                    }
                   break;
            }
            return false;
        }

        IEnumerable<string> PropertiesKeys()
        {
            if (((_flag0 & 0x1u) != 0))
            {
                yield return "owin.Version";
            }
            if (((_flag0 & 0x2u) != 0))
            {
                yield return "owin.CallCancelled";
            }
            if (((_flag0 & 0x4u) != 0))
            {
                yield return "owin.RequestProtocol";
            }
            if (((_flag0 & 0x8u) != 0))
            {
                yield return "owin.RequestMethod";
            }
            if (((_flag0 & 0x10u) != 0))
            {
                yield return "owin.RequestScheme";
            }
            if (((_flag0 & 0x20u) != 0))
            {
                yield return "owin.RequestPathBase";
            }
            if (((_flag0 & 0x40u) != 0))
            {
                yield return "owin.RequestPath";
            }
            if (((_flag0 & 0x80u) != 0))
            {
                yield return "owin.RequestQueryString";
            }
            if (((_flag0 & 0x100u) != 0))
            {
                yield return "owin.RequestHeaders";
            }
            if (((_flag0 & 0x200u) != 0))
            {
                yield return "owin.RequestBody";
            }
            if (((_flag0 & 0x400u) != 0))
            {
                yield return "owin.ResponseStatusCode";
            }
            if (((_flag0 & 0x800u) != 0))
            {
                yield return "owin.ResponseReasonPhrase";
            }
            if (((_flag0 & 0x1000u) != 0))
            {
                yield return "owin.ResponseHeaders";
            }
            if (((_flag0 & 0x2000u) != 0))
            {
                yield return "owin.ResponseBody";
            }
            if (((_flag0 & 0x4000u) != 0))
            {
                yield return "host.TraceOutput";
            }
            if (((_flag0 & 0x8000u) != 0))
            {
                yield return "server.DisableResponseBuffering";
            }
            if (((_flag0 & 0x10000u) != 0))
            {
                yield return "server.User";
            }
            if (((_flag0 & 0x20000u) != 0))
            {
                yield return "server.RemoteIpAddress";
            }
            if (((_flag0 & 0x40000u) != 0))
            {
                yield return "server.RemotePort";
            }
            if (((_flag0 & 0x80000u) != 0))
            {
                yield return "server.LocalIpAddress";
            }
            if (((_flag0 & 0x100000u) != 0))
            {
                yield return "server.LocalPort";
            }
            if (((_flag0 & 0x200000u) != 0))
            {
                yield return "server.IsLocal";
            }
            if (((_flag0 & 0x400000u) != 0))
            {
                yield return "websocket.Support";
            }
            if (((_flag0 & 0x800000u) != 0))
            {
                yield return "websocket.Accept";
            }
            if (((_flag0 & 0x1000000u) != 0))
            {
                yield return "sendfile.Version";
            }
            if (((_flag0 & 0x2000000u) != 0))
            {
                yield return "sendfile.Support";
            }
            if (((_flag0 & 0x4000000u) != 0))
            {
                yield return "sendfile.Func";
            }
            if (((_flag0 & 0x8000000u) != 0))
            {
                yield return "sendfile.Concurrency";
            }
            if (((_flag0 & 0x10000000u) != 0))
            {
                yield return "server.Name";
            }
            if (((_flag0 & 0x20000000u) != 0))
            {
                yield return "msaspnet.Version";
            }
            if (((_flag0 & 0x40000000u) != 0))
            {
                yield return "System.Web.Routing.RequestContext";
            }
            if (((_flag0 & 0x80000000u) != 0))
            {
                yield return "System.Web.HttpContextBase";
            }
        }

        IEnumerable<object> PropertiesValues()
        {
            if (((_flag0 & 0x1u) != 0))
            {
                yield return OwinVersion;
            }
            if (((_flag0 & 0x2u) != 0))
            {
                yield return CallCancelled;
            }
            if (((_flag0 & 0x4u) != 0))
            {
                yield return RequestProtocol;
            }
            if (((_flag0 & 0x8u) != 0))
            {
                yield return RequestMethod;
            }
            if (((_flag0 & 0x10u) != 0))
            {
                yield return RequestScheme;
            }
            if (((_flag0 & 0x20u) != 0))
            {
                yield return RequestPathBase;
            }
            if (((_flag0 & 0x40u) != 0))
            {
                yield return RequestPath;
            }
            if (((_flag0 & 0x80u) != 0))
            {
                yield return RequestQueryString;
            }
            if (((_flag0 & 0x100u) != 0))
            {
                yield return RequestHeaders;
            }
            if (((_flag0 & 0x200u) != 0))
            {
                yield return RequestBody;
            }
            if (((_flag0 & 0x400u) != 0))
            {
                yield return ResponseStatusCode;
            }
            if (((_flag0 & 0x800u) != 0))
            {
                yield return ResponseReasonPhrase;
            }
            if (((_flag0 & 0x1000u) != 0))
            {
                yield return ResponseHeaders;
            }
            if (((_flag0 & 0x2000u) != 0))
            {
                yield return ResponseBody;
            }
            if (((_flag0 & 0x4000u) != 0))
            {
                yield return HostTraceOutput;
            }
            if (((_flag0 & 0x8000u) != 0))
            {
                yield return ServerDisableResponseBuffering;
            }
            if (((_flag0 & 0x10000u) != 0))
            {
                yield return ServerUser;
            }
            if (((_flag0 & 0x20000u) != 0))
            {
                yield return ServerRemoteIpAddress;
            }
            if (((_flag0 & 0x40000u) != 0))
            {
                yield return ServerRemotePort;
            }
            if (((_flag0 & 0x80000u) != 0))
            {
                yield return ServerLocalIpAddress;
            }
            if (((_flag0 & 0x100000u) != 0))
            {
                yield return ServerLocalPort;
            }
            if (((_flag0 & 0x200000u) != 0))
            {
                yield return ServerIsLocal;
            }
            if (((_flag0 & 0x400000u) != 0))
            {
                yield return WebSocketSupport;
            }
            if (((_flag0 & 0x800000u) != 0))
            {
                yield return WebSocketAccept;
            }
            if (((_flag0 & 0x1000000u) != 0))
            {
                yield return SendFileVersion;
            }
            if (((_flag0 & 0x2000000u) != 0))
            {
                yield return SendFileSupport;
            }
            if (((_flag0 & 0x4000000u) != 0))
            {
                yield return SendFileFunc;
            }
            if (((_flag0 & 0x8000000u) != 0))
            {
                yield return SendFileConcurrency;
            }
            if (((_flag0 & 0x10000000u) != 0))
            {
                yield return ServerName;
            }
            if (((_flag0 & 0x20000000u) != 0))
            {
                yield return ServerVersion;
            }
            if (((_flag0 & 0x40000000u) != 0))
            {
                yield return RequestContext;
            }
            if (((_flag0 & 0x80000000u) != 0))
            {
                yield return HttpContextBase;
            }
        }

        IEnumerable<KeyValuePair<string,object>> PropertiesEnumerable()
        {
            if (((_flag0 & 0x1u) != 0))
            {
                yield return new KeyValuePair<string,object>("owin.Version", OwinVersion);
            }
            if (((_flag0 & 0x2u) != 0))
            {
                yield return new KeyValuePair<string,object>("owin.CallCancelled", CallCancelled);
            }
            if (((_flag0 & 0x4u) != 0))
            {
                yield return new KeyValuePair<string,object>("owin.RequestProtocol", RequestProtocol);
            }
            if (((_flag0 & 0x8u) != 0))
            {
                yield return new KeyValuePair<string,object>("owin.RequestMethod", RequestMethod);
            }
            if (((_flag0 & 0x10u) != 0))
            {
                yield return new KeyValuePair<string,object>("owin.RequestScheme", RequestScheme);
            }
            if (((_flag0 & 0x20u) != 0))
            {
                yield return new KeyValuePair<string,object>("owin.RequestPathBase", RequestPathBase);
            }
            if (((_flag0 & 0x40u) != 0))
            {
                yield return new KeyValuePair<string,object>("owin.RequestPath", RequestPath);
            }
            if (((_flag0 & 0x80u) != 0))
            {
                yield return new KeyValuePair<string,object>("owin.RequestQueryString", RequestQueryString);
            }
            if (((_flag0 & 0x100u) != 0))
            {
                yield return new KeyValuePair<string,object>("owin.RequestHeaders", RequestHeaders);
            }
            if (((_flag0 & 0x200u) != 0))
            {
                yield return new KeyValuePair<string,object>("owin.RequestBody", RequestBody);
            }
            if (((_flag0 & 0x400u) != 0))
            {
                yield return new KeyValuePair<string,object>("owin.ResponseStatusCode", ResponseStatusCode);
            }
            if (((_flag0 & 0x800u) != 0))
            {
                yield return new KeyValuePair<string,object>("owin.ResponseReasonPhrase", ResponseReasonPhrase);
            }
            if (((_flag0 & 0x1000u) != 0))
            {
                yield return new KeyValuePair<string,object>("owin.ResponseHeaders", ResponseHeaders);
            }
            if (((_flag0 & 0x2000u) != 0))
            {
                yield return new KeyValuePair<string,object>("owin.ResponseBody", ResponseBody);
            }
            if (((_flag0 & 0x4000u) != 0))
            {
                yield return new KeyValuePair<string,object>("host.TraceOutput", HostTraceOutput);
            }
            if (((_flag0 & 0x8000u) != 0))
            {
                yield return new KeyValuePair<string,object>("server.DisableResponseBuffering", ServerDisableResponseBuffering);
            }
            if (((_flag0 & 0x10000u) != 0))
            {
                yield return new KeyValuePair<string,object>("server.User", ServerUser);
            }
            if (((_flag0 & 0x20000u) != 0))
            {
                yield return new KeyValuePair<string,object>("server.RemoteIpAddress", ServerRemoteIpAddress);
            }
            if (((_flag0 & 0x40000u) != 0))
            {
                yield return new KeyValuePair<string,object>("server.RemotePort", ServerRemotePort);
            }
            if (((_flag0 & 0x80000u) != 0))
            {
                yield return new KeyValuePair<string,object>("server.LocalIpAddress", ServerLocalIpAddress);
            }
            if (((_flag0 & 0x100000u) != 0))
            {
                yield return new KeyValuePair<string,object>("server.LocalPort", ServerLocalPort);
            }
            if (((_flag0 & 0x200000u) != 0))
            {
                yield return new KeyValuePair<string,object>("server.IsLocal", ServerIsLocal);
            }
            if (((_flag0 & 0x400000u) != 0))
            {
                yield return new KeyValuePair<string,object>("websocket.Support", WebSocketSupport);
            }
            if (((_flag0 & 0x800000u) != 0))
            {
                yield return new KeyValuePair<string,object>("websocket.Accept", WebSocketAccept);
            }
            if (((_flag0 & 0x1000000u) != 0))
            {
                yield return new KeyValuePair<string,object>("sendfile.Version", SendFileVersion);
            }
            if (((_flag0 & 0x2000000u) != 0))
            {
                yield return new KeyValuePair<string,object>("sendfile.Support", SendFileSupport);
            }
            if (((_flag0 & 0x4000000u) != 0))
            {
                yield return new KeyValuePair<string,object>("sendfile.Func", SendFileFunc);
            }
            if (((_flag0 & 0x8000000u) != 0))
            {
                yield return new KeyValuePair<string,object>("sendfile.Concurrency", SendFileConcurrency);
            }
            if (((_flag0 & 0x10000000u) != 0))
            {
                yield return new KeyValuePair<string,object>("server.Name", ServerName);
            }
            if (((_flag0 & 0x20000000u) != 0))
            {
                yield return new KeyValuePair<string,object>("msaspnet.Version", ServerVersion);
            }
            if (((_flag0 & 0x40000000u) != 0))
            {
                yield return new KeyValuePair<string,object>("System.Web.Routing.RequestContext", RequestContext);
            }
            if (((_flag0 & 0x80000000u) != 0))
            {
                yield return new KeyValuePair<string,object>("System.Web.HttpContextBase", HttpContextBase);
            }
        }
	}
}
