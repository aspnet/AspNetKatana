




using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;

namespace Katana.Server.AspNet.CallEnvironment
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

        Action _HostDisableResponseBuffering;
        public Action HostDisableResponseBuffering 
        {
            get {return _HostDisableResponseBuffering;}
            set {_flag0 |= 0x200u; _HostDisableResponseBuffering = value;} 
        }

        System.Security.Principal.IPrincipal _HostUser;
        public System.Security.Principal.IPrincipal HostUser 
        {
            get {return _HostUser;}
            set {_flag0 |= 0x400u; _HostUser = value;} 
        }

        string _ServerVariableRemoteAddr;
        public string ServerVariableRemoteAddr 
        {
            get {return _ServerVariableRemoteAddr;}
            set {_flag0 |= 0x800u; _ServerVariableRemoteAddr = value;} 
        }

        string _ServerVariableRemoteHost;
        public string ServerVariableRemoteHost 
        {
            get {return _ServerVariableRemoteHost;}
            set {_flag0 |= 0x1000u; _ServerVariableRemoteHost = value;} 
        }

        string _ServerVariableRemotePort;
        public string ServerVariableRemotePort 
        {
            get {return _ServerVariableRemotePort;}
            set {_flag0 |= 0x2000u; _ServerVariableRemotePort = value;} 
        }

        string _ServerVariableLocalAddr;
        public string ServerVariableLocalAddr 
        {
            get {return _ServerVariableLocalAddr;}
            set {_flag0 |= 0x4000u; _ServerVariableLocalAddr = value;} 
        }

        string _ServerVariableServerPort;
        public string ServerVariableServerPort 
        {
            get {return _ServerVariableServerPort;}
            set {_flag0 |= 0x8000u; _ServerVariableServerPort = value;} 
        }

        RequestContext _RequestContext;
        public RequestContext RequestContext 
        {
            get {return _RequestContext;}
            set {_flag0 |= 0x10000u; _RequestContext = value;} 
        }

        HttpContextBase _HttpContextBase;
        public HttpContextBase HttpContextBase 
        {
            get {return _HttpContextBase;}
            set {_flag0 |= 0x20000u; _HttpContextBase = value;} 
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

                    if (((_flag0 & 0x800u) != 0) && string.Equals(key, "server.REMOTE_ADDR", StringComparison.Ordinal)) 
                    {
                        return true;
                    }

                    if (((_flag0 & 0x1000u) != 0) && string.Equals(key, "server.REMOTE_HOST", StringComparison.Ordinal)) 
                    {
                        return true;
                    }

                    if (((_flag0 & 0x2000u) != 0) && string.Equals(key, "server.REMOTE_PORT", StringComparison.Ordinal)) 
                    {
                        return true;
                    }

                    if (((_flag0 & 0x8000u) != 0) && string.Equals(key, "server.SERVER_PORT", StringComparison.Ordinal)) 
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

                case 29:

                    if (((_flag0 & 0x200u) != 0) && string.Equals(key, "host.DisableResponseBuffering", StringComparison.Ordinal)) 
                    {
                        return true;
                    }

                   break;

                case 9:

                    if (((_flag0 & 0x400u) != 0) && string.Equals(key, "host.User", StringComparison.Ordinal)) 
                    {
                        return true;
                    }

                   break;

                case 17:

                    if (((_flag0 & 0x4000u) != 0) && string.Equals(key, "server.LOCAL_ADDR", StringComparison.Ordinal)) 
                    {
                        return true;
                    }

                   break;

                case 33:

                    if (((_flag0 & 0x10000u) != 0) && string.Equals(key, "System.Web.Routing.RequestContext", StringComparison.Ordinal)) 
                    {
                        return true;
                    }

                   break;

                case 26:

                    if (((_flag0 & 0x20000u) != 0) && string.Equals(key, "System.Web.HttpContextBase", StringComparison.Ordinal)) 
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

                    if (((_flag0 & 0x800u) != 0) && string.Equals(key, "server.REMOTE_ADDR", StringComparison.Ordinal)) 
                    {
                        value = ServerVariableRemoteAddr;
                        return true;
                    }

                    if (((_flag0 & 0x1000u) != 0) && string.Equals(key, "server.REMOTE_HOST", StringComparison.Ordinal)) 
                    {
                        value = ServerVariableRemoteHost;
                        return true;
                    }

                    if (((_flag0 & 0x2000u) != 0) && string.Equals(key, "server.REMOTE_PORT", StringComparison.Ordinal)) 
                    {
                        value = ServerVariableRemotePort;
                        return true;
                    }

                    if (((_flag0 & 0x8000u) != 0) && string.Equals(key, "server.SERVER_PORT", StringComparison.Ordinal)) 
                    {
                        value = ServerVariableServerPort;
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

                case 29:

                    if (((_flag0 & 0x200u) != 0) && string.Equals(key, "host.DisableResponseBuffering", StringComparison.Ordinal)) 
                    {
                        value = HostDisableResponseBuffering;
                        return true;
                    }

                   break;

                case 9:

                    if (((_flag0 & 0x400u) != 0) && string.Equals(key, "host.User", StringComparison.Ordinal)) 
                    {
                        value = HostUser;
                        return true;
                    }

                   break;

                case 17:

                    if (((_flag0 & 0x4000u) != 0) && string.Equals(key, "server.LOCAL_ADDR", StringComparison.Ordinal)) 
                    {
                        value = ServerVariableLocalAddr;
                        return true;
                    }

                   break;

                case 33:

                    if (((_flag0 & 0x10000u) != 0) && string.Equals(key, "System.Web.Routing.RequestContext", StringComparison.Ordinal)) 
                    {
                        value = RequestContext;
                        return true;
                    }

                   break;

                case 26:

                    if (((_flag0 & 0x20000u) != 0) && string.Equals(key, "System.Web.HttpContextBase", StringComparison.Ordinal)) 
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

                    if (string.Equals(key, "server.REMOTE_ADDR", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x800u;
                        ServerVariableRemoteAddr = (string)value;
                        return true;
                    }

                    if (string.Equals(key, "server.REMOTE_HOST", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x1000u;
                        ServerVariableRemoteHost = (string)value;
                        return true;
                    }

                    if (string.Equals(key, "server.REMOTE_PORT", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x2000u;
                        ServerVariableRemotePort = (string)value;
                        return true;
                    }

                    if (string.Equals(key, "server.SERVER_PORT", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x8000u;
                        ServerVariableServerPort = (string)value;
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

                case 29:

                    if (string.Equals(key, "host.DisableResponseBuffering", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x200u;
                        HostDisableResponseBuffering = (Action)value;
                        return true;
                    }

                   break;

                case 9:

                    if (string.Equals(key, "host.User", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x400u;
                        HostUser = (System.Security.Principal.IPrincipal)value;
                        return true;
                    }

                   break;

                case 17:

                    if (string.Equals(key, "server.LOCAL_ADDR", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x4000u;
                        ServerVariableLocalAddr = (string)value;
                        return true;
                    }

                   break;

                case 33:

                    if (string.Equals(key, "System.Web.Routing.RequestContext", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x10000u;
                        RequestContext = (RequestContext)value;
                        return true;
                    }

                   break;

                case 26:

                    if (string.Equals(key, "System.Web.HttpContextBase", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x20000u;
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

                    if (((_flag0 & 0x800u) != 0) && string.Equals(key, "server.REMOTE_ADDR", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x800u;
                        ServerVariableRemoteAddr = default(string);
                        return true;
                    }

                    if (((_flag0 & 0x1000u) != 0) && string.Equals(key, "server.REMOTE_HOST", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x1000u;
                        ServerVariableRemoteHost = default(string);
                        return true;
                    }

                    if (((_flag0 & 0x2000u) != 0) && string.Equals(key, "server.REMOTE_PORT", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x2000u;
                        ServerVariableRemotePort = default(string);
                        return true;
                    }

                    if (((_flag0 & 0x8000u) != 0) && string.Equals(key, "server.SERVER_PORT", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x8000u;
                        ServerVariableServerPort = default(string);
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

                case 29:

                    if (((_flag0 & 0x200u) != 0) && string.Equals(key, "host.DisableResponseBuffering", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x200u;
                        HostDisableResponseBuffering = default(Action);
                        return true;
                    }

                   break;

                case 9:

                    if (((_flag0 & 0x400u) != 0) && string.Equals(key, "host.User", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x400u;
                        HostUser = default(System.Security.Principal.IPrincipal);
                        return true;
                    }

                   break;

                case 17:

                    if (((_flag0 & 0x4000u) != 0) && string.Equals(key, "server.LOCAL_ADDR", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x4000u;
                        ServerVariableLocalAddr = default(string);
                        return true;
                    }

                   break;

                case 33:

                    if (((_flag0 & 0x10000u) != 0) && string.Equals(key, "System.Web.Routing.RequestContext", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x10000u;
                        RequestContext = default(RequestContext);
                        return true;
                    }

                   break;

                case 26:

                    if (((_flag0 & 0x20000u) != 0) && string.Equals(key, "System.Web.HttpContextBase", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x20000u;
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
                yield return "host.DisableResponseBuffering";
            }

            if (((_flag0 & 0x400u) != 0))
            {
                yield return "host.User";
            }

            if (((_flag0 & 0x800u) != 0))
            {
                yield return "server.REMOTE_ADDR";
            }

            if (((_flag0 & 0x1000u) != 0))
            {
                yield return "server.REMOTE_HOST";
            }

            if (((_flag0 & 0x2000u) != 0))
            {
                yield return "server.REMOTE_PORT";
            }

            if (((_flag0 & 0x4000u) != 0))
            {
                yield return "server.LOCAL_ADDR";
            }

            if (((_flag0 & 0x8000u) != 0))
            {
                yield return "server.SERVER_PORT";
            }

            if (((_flag0 & 0x10000u) != 0))
            {
                yield return "System.Web.Routing.RequestContext";
            }

            if (((_flag0 & 0x20000u) != 0))
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
                yield return HostDisableResponseBuffering;
            }

            if (((_flag0 & 0x400u) != 0))
            {
                yield return HostUser;
            }

            if (((_flag0 & 0x800u) != 0))
            {
                yield return ServerVariableRemoteAddr;
            }

            if (((_flag0 & 0x1000u) != 0))
            {
                yield return ServerVariableRemoteHost;
            }

            if (((_flag0 & 0x2000u) != 0))
            {
                yield return ServerVariableRemotePort;
            }

            if (((_flag0 & 0x4000u) != 0))
            {
                yield return ServerVariableLocalAddr;
            }

            if (((_flag0 & 0x8000u) != 0))
            {
                yield return ServerVariableServerPort;
            }

            if (((_flag0 & 0x10000u) != 0))
            {
                yield return RequestContext;
            }

            if (((_flag0 & 0x20000u) != 0))
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
                yield return new KeyValuePair<string,object>("host.DisableResponseBuffering", HostDisableResponseBuffering);
            }

            if (((_flag0 & 0x400u) != 0))
            {
                yield return new KeyValuePair<string,object>("host.User", HostUser);
            }

            if (((_flag0 & 0x800u) != 0))
            {
                yield return new KeyValuePair<string,object>("server.REMOTE_ADDR", ServerVariableRemoteAddr);
            }

            if (((_flag0 & 0x1000u) != 0))
            {
                yield return new KeyValuePair<string,object>("server.REMOTE_HOST", ServerVariableRemoteHost);
            }

            if (((_flag0 & 0x2000u) != 0))
            {
                yield return new KeyValuePair<string,object>("server.REMOTE_PORT", ServerVariableRemotePort);
            }

            if (((_flag0 & 0x4000u) != 0))
            {
                yield return new KeyValuePair<string,object>("server.LOCAL_ADDR", ServerVariableLocalAddr);
            }

            if (((_flag0 & 0x8000u) != 0))
            {
                yield return new KeyValuePair<string,object>("server.SERVER_PORT", ServerVariableServerPort);
            }

            if (((_flag0 & 0x10000u) != 0))
            {
                yield return new KeyValuePair<string,object>("System.Web.Routing.RequestContext", RequestContext);
            }

            if (((_flag0 & 0x20000u) != 0))
            {
                yield return new KeyValuePair<string,object>("System.Web.HttpContextBase", HttpContextBase);
            }

        }
	}
}
