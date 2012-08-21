
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
        string _HttpVersion;
        public string HttpVersion 
        {
            get {return _HttpVersion;}
            set {_flag0 |= 0x2u; _HttpVersion = value;} 
        }
        string _RequestMethod;
        public string RequestMethod 
        {
            get {return _RequestMethod;}
            set {_flag0 |= 0x4u; _RequestMethod = value;} 
        }
        string _RequestScheme;
        public string RequestScheme 
        {
            get {return _RequestScheme;}
            set {_flag0 |= 0x8u; _RequestScheme = value;} 
        }
        string _RequestPathBase;
        public string RequestPathBase 
        {
            get {return _RequestPathBase;}
            set {_flag0 |= 0x10u; _RequestPathBase = value;} 
        }
        string _RequestPath;
        public string RequestPath 
        {
            get {return _RequestPath;}
            set {_flag0 |= 0x20u; _RequestPath = value;} 
        }
        string _RequestQueryString;
        public string RequestQueryString 
        {
            get {return _RequestQueryString;}
            set {_flag0 |= 0x40u; _RequestQueryString = value;} 
        }
        Task _CallCompleted;
        public Task CallCompleted 
        {
            get {return _CallCompleted;}
            set {_flag0 |= 0x80u; _CallCompleted = value;} 
        }
        TextWriter _HostTraceOutput;
        public TextWriter HostTraceOutput 
        {
            get {return _HostTraceOutput;}
            set {_flag0 |= 0x100u; _HostTraceOutput = value;} 
        }
        Action _ServerDisableResponseBuffering;
        public Action ServerDisableResponseBuffering 
        {
            get {return _ServerDisableResponseBuffering;}
            set {_flag0 |= 0x200u; _ServerDisableResponseBuffering = value;} 
        }
        System.Security.Principal.IPrincipal _ServerUser;
        public System.Security.Principal.IPrincipal ServerUser 
        {
            get {return _ServerUser;}
            set {_flag0 |= 0x400u; _ServerUser = value;} 
        }
        string _ServerRemoteIpAddress;
        public string ServerRemoteIpAddress 
        {
            get {return _ServerRemoteIpAddress;}
            set {_flag0 |= 0x800u; _ServerRemoteIpAddress = value;} 
        }
        string _ServerRemotePort;
        public string ServerRemotePort 
        {
            get {return _ServerRemotePort;}
            set {_flag0 |= 0x1000u; _ServerRemotePort = value;} 
        }
        string _ServerLocalIpAddress;
        public string ServerLocalIpAddress 
        {
            get {return _ServerLocalIpAddress;}
            set {_flag0 |= 0x2000u; _ServerLocalIpAddress = value;} 
        }
        string _ServerLocalPort;
        public string ServerLocalPort 
        {
            get {return _ServerLocalPort;}
            set {_flag0 |= 0x4000u; _ServerLocalPort = value;} 
        }
        bool _ServerIsLocal;
        public bool ServerIsLocal 
        {
            get {return _ServerIsLocal;}
            set {_flag0 |= 0x8000u; _ServerIsLocal = value;} 
        }
        string _WebSocketSupport;
        public string WebSocketSupport 
        {
            get {return _WebSocketSupport;}
            set {_flag0 |= 0x10000u; _WebSocketSupport = value;} 
        }
        RequestContext _RequestContext;
        public RequestContext RequestContext 
        {
            get {return _RequestContext;}
            set {_flag0 |= 0x20000u; _RequestContext = value;} 
        }
        HttpContextBase _HttpContextBase;
        public HttpContextBase HttpContextBase 
        {
            get {return _HttpContextBase;}
            set {_flag0 |= 0x40000u; _HttpContextBase = value;} 
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
                case 16:
                    if (((_flag0 & 0x2u) != 0) && string.Equals(key, "owin.HttpVersion", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                    if (((_flag0 & 0x20u) != 0) && string.Equals(key, "owin.RequestPath", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                    if (((_flag0 & 0x100u) != 0) && string.Equals(key, "host.TraceOutput", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                    if (((_flag0 & 0x4000u) != 0) && string.Equals(key, "server.LocalPort", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                   break;
                case 18:
                    if (((_flag0 & 0x4u) != 0) && string.Equals(key, "owin.RequestMethod", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                    if (((_flag0 & 0x8u) != 0) && string.Equals(key, "owin.RequestScheme", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                    if (((_flag0 & 0x80u) != 0) && string.Equals(key, "owin.CallCompleted", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                   break;
                case 20:
                    if (((_flag0 & 0x10u) != 0) && string.Equals(key, "owin.RequestPathBase", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                   break;
                case 23:
                    if (((_flag0 & 0x40u) != 0) && string.Equals(key, "owin.RequestQueryString", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                   break;
                case 31:
                    if (((_flag0 & 0x200u) != 0) && string.Equals(key, "server.DisableResponseBuffering", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                   break;
                case 11:
                    if (((_flag0 & 0x400u) != 0) && string.Equals(key, "server.User", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                   break;
                case 22:
                    if (((_flag0 & 0x800u) != 0) && string.Equals(key, "server.RemoteIpAddress", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                   break;
                case 17:
                    if (((_flag0 & 0x1000u) != 0) && string.Equals(key, "server.RemotePort", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                    if (((_flag0 & 0x10000u) != 0) && string.Equals(key, "websocket.Support", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                   break;
                case 21:
                    if (((_flag0 & 0x2000u) != 0) && string.Equals(key, "server.LocalIpAddress", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                   break;
                case 14:
                    if (((_flag0 & 0x8000u) != 0) && string.Equals(key, "server.IsLocal", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                   break;
                case 33:
                    if (((_flag0 & 0x20000u) != 0) && string.Equals(key, "System.Web.Routing.RequestContext", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                   break;
                case 26:
                    if (((_flag0 & 0x40000u) != 0) && string.Equals(key, "System.Web.HttpContextBase", StringComparison.Ordinal)) 
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
                case 16:
                    if (((_flag0 & 0x2u) != 0) && string.Equals(key, "owin.HttpVersion", StringComparison.Ordinal)) 
                    {
                        value = HttpVersion;
                        return true;
                    }
                    if (((_flag0 & 0x20u) != 0) && string.Equals(key, "owin.RequestPath", StringComparison.Ordinal)) 
                    {
                        value = RequestPath;
                        return true;
                    }
                    if (((_flag0 & 0x100u) != 0) && string.Equals(key, "host.TraceOutput", StringComparison.Ordinal)) 
                    {
                        value = HostTraceOutput;
                        return true;
                    }
                    if (((_flag0 & 0x4000u) != 0) && string.Equals(key, "server.LocalPort", StringComparison.Ordinal)) 
                    {
                        value = ServerLocalPort;
                        return true;
                    }
                   break;
                case 18:
                    if (((_flag0 & 0x4u) != 0) && string.Equals(key, "owin.RequestMethod", StringComparison.Ordinal)) 
                    {
                        value = RequestMethod;
                        return true;
                    }
                    if (((_flag0 & 0x8u) != 0) && string.Equals(key, "owin.RequestScheme", StringComparison.Ordinal)) 
                    {
                        value = RequestScheme;
                        return true;
                    }
                    if (((_flag0 & 0x80u) != 0) && string.Equals(key, "owin.CallCompleted", StringComparison.Ordinal)) 
                    {
                        value = CallCompleted;
                        return true;
                    }
                   break;
                case 20:
                    if (((_flag0 & 0x10u) != 0) && string.Equals(key, "owin.RequestPathBase", StringComparison.Ordinal)) 
                    {
                        value = RequestPathBase;
                        return true;
                    }
                   break;
                case 23:
                    if (((_flag0 & 0x40u) != 0) && string.Equals(key, "owin.RequestQueryString", StringComparison.Ordinal)) 
                    {
                        value = RequestQueryString;
                        return true;
                    }
                   break;
                case 31:
                    if (((_flag0 & 0x200u) != 0) && string.Equals(key, "server.DisableResponseBuffering", StringComparison.Ordinal)) 
                    {
                        value = ServerDisableResponseBuffering;
                        return true;
                    }
                   break;
                case 11:
                    if (((_flag0 & 0x400u) != 0) && string.Equals(key, "server.User", StringComparison.Ordinal)) 
                    {
                        value = ServerUser;
                        return true;
                    }
                   break;
                case 22:
                    if (((_flag0 & 0x800u) != 0) && string.Equals(key, "server.RemoteIpAddress", StringComparison.Ordinal)) 
                    {
                        value = ServerRemoteIpAddress;
                        return true;
                    }
                   break;
                case 17:
                    if (((_flag0 & 0x1000u) != 0) && string.Equals(key, "server.RemotePort", StringComparison.Ordinal)) 
                    {
                        value = ServerRemotePort;
                        return true;
                    }
                    if (((_flag0 & 0x10000u) != 0) && string.Equals(key, "websocket.Support", StringComparison.Ordinal)) 
                    {
                        value = WebSocketSupport;
                        return true;
                    }
                   break;
                case 21:
                    if (((_flag0 & 0x2000u) != 0) && string.Equals(key, "server.LocalIpAddress", StringComparison.Ordinal)) 
                    {
                        value = ServerLocalIpAddress;
                        return true;
                    }
                   break;
                case 14:
                    if (((_flag0 & 0x8000u) != 0) && string.Equals(key, "server.IsLocal", StringComparison.Ordinal)) 
                    {
                        value = ServerIsLocal;
                        return true;
                    }
                   break;
                case 33:
                    if (((_flag0 & 0x20000u) != 0) && string.Equals(key, "System.Web.Routing.RequestContext", StringComparison.Ordinal)) 
                    {
                        value = RequestContext;
                        return true;
                    }
                   break;
                case 26:
                    if (((_flag0 & 0x40000u) != 0) && string.Equals(key, "System.Web.HttpContextBase", StringComparison.Ordinal)) 
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
                case 16:
                    if (string.Equals(key, "owin.HttpVersion", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x2u;
                        HttpVersion = (string)value;
                        return true;
                    }
                    if (string.Equals(key, "owin.RequestPath", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x20u;
                        RequestPath = (string)value;
                        return true;
                    }
                    if (string.Equals(key, "host.TraceOutput", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x100u;
                        HostTraceOutput = (TextWriter)value;
                        return true;
                    }
                    if (string.Equals(key, "server.LocalPort", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x4000u;
                        ServerLocalPort = (string)value;
                        return true;
                    }
                   break;
                case 18:
                    if (string.Equals(key, "owin.RequestMethod", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x4u;
                        RequestMethod = (string)value;
                        return true;
                    }
                    if (string.Equals(key, "owin.RequestScheme", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x8u;
                        RequestScheme = (string)value;
                        return true;
                    }
                    if (string.Equals(key, "owin.CallCompleted", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x80u;
                        CallCompleted = (Task)value;
                        return true;
                    }
                   break;
                case 20:
                    if (string.Equals(key, "owin.RequestPathBase", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x10u;
                        RequestPathBase = (string)value;
                        return true;
                    }
                   break;
                case 23:
                    if (string.Equals(key, "owin.RequestQueryString", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x40u;
                        RequestQueryString = (string)value;
                        return true;
                    }
                   break;
                case 31:
                    if (string.Equals(key, "server.DisableResponseBuffering", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x200u;
                        ServerDisableResponseBuffering = (Action)value;
                        return true;
                    }
                   break;
                case 11:
                    if (string.Equals(key, "server.User", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x400u;
                        ServerUser = (System.Security.Principal.IPrincipal)value;
                        return true;
                    }
                   break;
                case 22:
                    if (string.Equals(key, "server.RemoteIpAddress", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x800u;
                        ServerRemoteIpAddress = (string)value;
                        return true;
                    }
                   break;
                case 17:
                    if (string.Equals(key, "server.RemotePort", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x1000u;
                        ServerRemotePort = (string)value;
                        return true;
                    }
                    if (string.Equals(key, "websocket.Support", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x10000u;
                        WebSocketSupport = (string)value;
                        return true;
                    }
                   break;
                case 21:
                    if (string.Equals(key, "server.LocalIpAddress", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x2000u;
                        ServerLocalIpAddress = (string)value;
                        return true;
                    }
                   break;
                case 14:
                    if (string.Equals(key, "server.IsLocal", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x8000u;
                        ServerIsLocal = (bool)value;
                        return true;
                    }
                   break;
                case 33:
                    if (string.Equals(key, "System.Web.Routing.RequestContext", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x20000u;
                        RequestContext = (RequestContext)value;
                        return true;
                    }
                   break;
                case 26:
                    if (string.Equals(key, "System.Web.HttpContextBase", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x40000u;
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
                case 16:
                    if (((_flag0 & 0x2u) != 0) && string.Equals(key, "owin.HttpVersion", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x2u;
                        HttpVersion = default(string);
                        return true;
                    }
                    if (((_flag0 & 0x20u) != 0) && string.Equals(key, "owin.RequestPath", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x20u;
                        RequestPath = default(string);
                        return true;
                    }
                    if (((_flag0 & 0x100u) != 0) && string.Equals(key, "host.TraceOutput", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x100u;
                        HostTraceOutput = default(TextWriter);
                        return true;
                    }
                    if (((_flag0 & 0x4000u) != 0) && string.Equals(key, "server.LocalPort", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x4000u;
                        ServerLocalPort = default(string);
                        return true;
                    }
                   break;
                case 18:
                    if (((_flag0 & 0x4u) != 0) && string.Equals(key, "owin.RequestMethod", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x4u;
                        RequestMethod = default(string);
                        return true;
                    }
                    if (((_flag0 & 0x8u) != 0) && string.Equals(key, "owin.RequestScheme", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x8u;
                        RequestScheme = default(string);
                        return true;
                    }
                    if (((_flag0 & 0x80u) != 0) && string.Equals(key, "owin.CallCompleted", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x80u;
                        CallCompleted = default(Task);
                        return true;
                    }
                   break;
                case 20:
                    if (((_flag0 & 0x10u) != 0) && string.Equals(key, "owin.RequestPathBase", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x10u;
                        RequestPathBase = default(string);
                        return true;
                    }
                   break;
                case 23:
                    if (((_flag0 & 0x40u) != 0) && string.Equals(key, "owin.RequestQueryString", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x40u;
                        RequestQueryString = default(string);
                        return true;
                    }
                   break;
                case 31:
                    if (((_flag0 & 0x200u) != 0) && string.Equals(key, "server.DisableResponseBuffering", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x200u;
                        ServerDisableResponseBuffering = default(Action);
                        return true;
                    }
                   break;
                case 11:
                    if (((_flag0 & 0x400u) != 0) && string.Equals(key, "server.User", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x400u;
                        ServerUser = default(System.Security.Principal.IPrincipal);
                        return true;
                    }
                   break;
                case 22:
                    if (((_flag0 & 0x800u) != 0) && string.Equals(key, "server.RemoteIpAddress", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x800u;
                        ServerRemoteIpAddress = default(string);
                        return true;
                    }
                   break;
                case 17:
                    if (((_flag0 & 0x1000u) != 0) && string.Equals(key, "server.RemotePort", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x1000u;
                        ServerRemotePort = default(string);
                        return true;
                    }
                    if (((_flag0 & 0x10000u) != 0) && string.Equals(key, "websocket.Support", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x10000u;
                        WebSocketSupport = default(string);
                        return true;
                    }
                   break;
                case 21:
                    if (((_flag0 & 0x2000u) != 0) && string.Equals(key, "server.LocalIpAddress", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x2000u;
                        ServerLocalIpAddress = default(string);
                        return true;
                    }
                   break;
                case 14:
                    if (((_flag0 & 0x8000u) != 0) && string.Equals(key, "server.IsLocal", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x8000u;
                        ServerIsLocal = default(bool);
                        return true;
                    }
                   break;
                case 33:
                    if (((_flag0 & 0x20000u) != 0) && string.Equals(key, "System.Web.Routing.RequestContext", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x20000u;
                        RequestContext = default(RequestContext);
                        return true;
                    }
                   break;
                case 26:
                    if (((_flag0 & 0x40000u) != 0) && string.Equals(key, "System.Web.HttpContextBase", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x40000u;
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
                yield return "owin.HttpVersion";
            }
            if (((_flag0 & 0x4u) != 0))
            {
                yield return "owin.RequestMethod";
            }
            if (((_flag0 & 0x8u) != 0))
            {
                yield return "owin.RequestScheme";
            }
            if (((_flag0 & 0x10u) != 0))
            {
                yield return "owin.RequestPathBase";
            }
            if (((_flag0 & 0x20u) != 0))
            {
                yield return "owin.RequestPath";
            }
            if (((_flag0 & 0x40u) != 0))
            {
                yield return "owin.RequestQueryString";
            }
            if (((_flag0 & 0x80u) != 0))
            {
                yield return "owin.CallCompleted";
            }
            if (((_flag0 & 0x100u) != 0))
            {
                yield return "host.TraceOutput";
            }
            if (((_flag0 & 0x200u) != 0))
            {
                yield return "server.DisableResponseBuffering";
            }
            if (((_flag0 & 0x400u) != 0))
            {
                yield return "server.User";
            }
            if (((_flag0 & 0x800u) != 0))
            {
                yield return "server.RemoteIpAddress";
            }
            if (((_flag0 & 0x1000u) != 0))
            {
                yield return "server.RemotePort";
            }
            if (((_flag0 & 0x2000u) != 0))
            {
                yield return "server.LocalIpAddress";
            }
            if (((_flag0 & 0x4000u) != 0))
            {
                yield return "server.LocalPort";
            }
            if (((_flag0 & 0x8000u) != 0))
            {
                yield return "server.IsLocal";
            }
            if (((_flag0 & 0x10000u) != 0))
            {
                yield return "websocket.Support";
            }
            if (((_flag0 & 0x20000u) != 0))
            {
                yield return "System.Web.Routing.RequestContext";
            }
            if (((_flag0 & 0x40000u) != 0))
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
                yield return HttpVersion;
            }
            if (((_flag0 & 0x4u) != 0))
            {
                yield return RequestMethod;
            }
            if (((_flag0 & 0x8u) != 0))
            {
                yield return RequestScheme;
            }
            if (((_flag0 & 0x10u) != 0))
            {
                yield return RequestPathBase;
            }
            if (((_flag0 & 0x20u) != 0))
            {
                yield return RequestPath;
            }
            if (((_flag0 & 0x40u) != 0))
            {
                yield return RequestQueryString;
            }
            if (((_flag0 & 0x80u) != 0))
            {
                yield return CallCompleted;
            }
            if (((_flag0 & 0x100u) != 0))
            {
                yield return HostTraceOutput;
            }
            if (((_flag0 & 0x200u) != 0))
            {
                yield return ServerDisableResponseBuffering;
            }
            if (((_flag0 & 0x400u) != 0))
            {
                yield return ServerUser;
            }
            if (((_flag0 & 0x800u) != 0))
            {
                yield return ServerRemoteIpAddress;
            }
            if (((_flag0 & 0x1000u) != 0))
            {
                yield return ServerRemotePort;
            }
            if (((_flag0 & 0x2000u) != 0))
            {
                yield return ServerLocalIpAddress;
            }
            if (((_flag0 & 0x4000u) != 0))
            {
                yield return ServerLocalPort;
            }
            if (((_flag0 & 0x8000u) != 0))
            {
                yield return ServerIsLocal;
            }
            if (((_flag0 & 0x10000u) != 0))
            {
                yield return WebSocketSupport;
            }
            if (((_flag0 & 0x20000u) != 0))
            {
                yield return RequestContext;
            }
            if (((_flag0 & 0x40000u) != 0))
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
                yield return new KeyValuePair<string,object>("owin.HttpVersion", HttpVersion);
            }
            if (((_flag0 & 0x4u) != 0))
            {
                yield return new KeyValuePair<string,object>("owin.RequestMethod", RequestMethod);
            }
            if (((_flag0 & 0x8u) != 0))
            {
                yield return new KeyValuePair<string,object>("owin.RequestScheme", RequestScheme);
            }
            if (((_flag0 & 0x10u) != 0))
            {
                yield return new KeyValuePair<string,object>("owin.RequestPathBase", RequestPathBase);
            }
            if (((_flag0 & 0x20u) != 0))
            {
                yield return new KeyValuePair<string,object>("owin.RequestPath", RequestPath);
            }
            if (((_flag0 & 0x40u) != 0))
            {
                yield return new KeyValuePair<string,object>("owin.RequestQueryString", RequestQueryString);
            }
            if (((_flag0 & 0x80u) != 0))
            {
                yield return new KeyValuePair<string,object>("owin.CallCompleted", CallCompleted);
            }
            if (((_flag0 & 0x100u) != 0))
            {
                yield return new KeyValuePair<string,object>("host.TraceOutput", HostTraceOutput);
            }
            if (((_flag0 & 0x200u) != 0))
            {
                yield return new KeyValuePair<string,object>("server.DisableResponseBuffering", ServerDisableResponseBuffering);
            }
            if (((_flag0 & 0x400u) != 0))
            {
                yield return new KeyValuePair<string,object>("server.User", ServerUser);
            }
            if (((_flag0 & 0x800u) != 0))
            {
                yield return new KeyValuePair<string,object>("server.RemoteIpAddress", ServerRemoteIpAddress);
            }
            if (((_flag0 & 0x1000u) != 0))
            {
                yield return new KeyValuePair<string,object>("server.RemotePort", ServerRemotePort);
            }
            if (((_flag0 & 0x2000u) != 0))
            {
                yield return new KeyValuePair<string,object>("server.LocalIpAddress", ServerLocalIpAddress);
            }
            if (((_flag0 & 0x4000u) != 0))
            {
                yield return new KeyValuePair<string,object>("server.LocalPort", ServerLocalPort);
            }
            if (((_flag0 & 0x8000u) != 0))
            {
                yield return new KeyValuePair<string,object>("server.IsLocal", ServerIsLocal);
            }
            if (((_flag0 & 0x10000u) != 0))
            {
                yield return new KeyValuePair<string,object>("websocket.Support", WebSocketSupport);
            }
            if (((_flag0 & 0x20000u) != 0))
            {
                yield return new KeyValuePair<string,object>("System.Web.Routing.RequestContext", RequestContext);
            }
            if (((_flag0 & 0x40000u) != 0))
            {
                yield return new KeyValuePair<string,object>("System.Web.HttpContextBase", HttpContextBase);
            }
        }
	}
}
