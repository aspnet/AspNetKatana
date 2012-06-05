
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
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
        string _RequestMethod;
        public string RequestMethod 
        {
            get {return _RequestMethod;}
            set {_flag0 |= 0x2u; _RequestMethod = value;} 
        }
        string _RequestScheme;
        public string RequestScheme 
        {
            get {return _RequestScheme;}
            set {_flag0 |= 0x4u; _RequestScheme = value;} 
        }
        string _RequestPathBase;
        public string RequestPathBase 
        {
            get {return _RequestPathBase;}
            set {_flag0 |= 0x8u; _RequestPathBase = value;} 
        }
        string _RequestPath;
        public string RequestPath 
        {
            get {return _RequestPath;}
            set {_flag0 |= 0x10u; _RequestPath = value;} 
        }
        string _RequestQueryString;
        public string RequestQueryString 
        {
            get {return _RequestQueryString;}
            set {_flag0 |= 0x20u; _RequestQueryString = value;} 
        }
        IDictionary<string, string[]> _RequestHeaders;
        public IDictionary<string, string[]> RequestHeaders 
        {
            get {return _RequestHeaders;}
            set {_flag0 |= 0x40u; _RequestHeaders = value;} 
        }
        object _RequestBody;
        public object RequestBody 
        {
            get {return _RequestBody;}
            set {_flag0 |= 0x80u; _RequestBody = value;} 
        }
        CancellationToken _CallDisposed;
        public CancellationToken CallDisposed 
        {
            get {return _CallDisposed;}
            set {_flag0 |= 0x100u; _CallDisposed = value;} 
        }
        TextWriter _TraceOutput;
        public TextWriter TraceOutput 
        {
            get {return _TraceOutput;}
            set {_flag0 |= 0x200u; _TraceOutput = value;} 
        }
        RequestContext _RequestContext;
        public RequestContext RequestContext 
        {
            get {return _RequestContext;}
            set {_flag0 |= 0x400u; _RequestContext = value;} 
        }
        HttpContextBase _HttpContextBase;
        public HttpContextBase HttpContextBase 
        {
            get {return _HttpContextBase;}
            set {_flag0 |= 0x800u; _HttpContextBase = value;} 
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
                    if (((_flag0 & 0x2u) != 0) && string.Equals(key, "owin.RequestMethod", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                    if (((_flag0 & 0x4u) != 0) && string.Equals(key, "owin.RequestScheme", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                   break;
                case 20:
                    if (((_flag0 & 0x8u) != 0) && string.Equals(key, "owin.RequestPathBase", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                   break;
                case 16:
                    if (((_flag0 & 0x10u) != 0) && string.Equals(key, "owin.RequestPath", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                    if (((_flag0 & 0x80u) != 0) && string.Equals(key, "owin.RequestBody", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                    if (((_flag0 & 0x200u) != 0) && string.Equals(key, "host.TraceOutput", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                   break;
                case 23:
                    if (((_flag0 & 0x20u) != 0) && string.Equals(key, "owin.RequestQueryString", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                   break;
                case 19:
                    if (((_flag0 & 0x40u) != 0) && string.Equals(key, "owin.RequestHeaders", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                   break;
                case 17:
                    if (((_flag0 & 0x100u) != 0) && string.Equals(key, "host.CallDisposed", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                   break;
                case 33:
                    if (((_flag0 & 0x400u) != 0) && string.Equals(key, "System.Web.Routing.RequestContext", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                   break;
                case 26:
                    if (((_flag0 & 0x800u) != 0) && string.Equals(key, "System.Web.HttpContextBase", StringComparison.Ordinal)) 
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
                    if (((_flag0 & 0x2u) != 0) && string.Equals(key, "owin.RequestMethod", StringComparison.Ordinal)) 
                    {
                        value = RequestMethod;
                        return true;
                    }
                    if (((_flag0 & 0x4u) != 0) && string.Equals(key, "owin.RequestScheme", StringComparison.Ordinal)) 
                    {
                        value = RequestScheme;
                        return true;
                    }
                   break;
                case 20:
                    if (((_flag0 & 0x8u) != 0) && string.Equals(key, "owin.RequestPathBase", StringComparison.Ordinal)) 
                    {
                        value = RequestPathBase;
                        return true;
                    }
                   break;
                case 16:
                    if (((_flag0 & 0x10u) != 0) && string.Equals(key, "owin.RequestPath", StringComparison.Ordinal)) 
                    {
                        value = RequestPath;
                        return true;
                    }
                    if (((_flag0 & 0x80u) != 0) && string.Equals(key, "owin.RequestBody", StringComparison.Ordinal)) 
                    {
                        value = RequestBody;
                        return true;
                    }
                    if (((_flag0 & 0x200u) != 0) && string.Equals(key, "host.TraceOutput", StringComparison.Ordinal)) 
                    {
                        value = TraceOutput;
                        return true;
                    }
                   break;
                case 23:
                    if (((_flag0 & 0x20u) != 0) && string.Equals(key, "owin.RequestQueryString", StringComparison.Ordinal)) 
                    {
                        value = RequestQueryString;
                        return true;
                    }
                   break;
                case 19:
                    if (((_flag0 & 0x40u) != 0) && string.Equals(key, "owin.RequestHeaders", StringComparison.Ordinal)) 
                    {
                        value = RequestHeaders;
                        return true;
                    }
                   break;
                case 17:
                    if (((_flag0 & 0x100u) != 0) && string.Equals(key, "host.CallDisposed", StringComparison.Ordinal)) 
                    {
                        value = CallDisposed;
                        return true;
                    }
                   break;
                case 33:
                    if (((_flag0 & 0x400u) != 0) && string.Equals(key, "System.Web.Routing.RequestContext", StringComparison.Ordinal)) 
                    {
                        value = RequestContext;
                        return true;
                    }
                   break;
                case 26:
                    if (((_flag0 & 0x800u) != 0) && string.Equals(key, "System.Web.HttpContextBase", StringComparison.Ordinal)) 
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
                    if (string.Equals(key, "owin.RequestMethod", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x2u;
                        RequestMethod = (string)value;
                        return true;
                    }
                    if (string.Equals(key, "owin.RequestScheme", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x4u;
                        RequestScheme = (string)value;
                        return true;
                    }
                   break;
                case 20:
                    if (string.Equals(key, "owin.RequestPathBase", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x8u;
                        RequestPathBase = (string)value;
                        return true;
                    }
                   break;
                case 16:
                    if (string.Equals(key, "owin.RequestPath", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x10u;
                        RequestPath = (string)value;
                        return true;
                    }
                    if (string.Equals(key, "owin.RequestBody", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x80u;
                        RequestBody = (object)value;
                        return true;
                    }
                    if (string.Equals(key, "host.TraceOutput", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x200u;
                        TraceOutput = (TextWriter)value;
                        return true;
                    }
                   break;
                case 23:
                    if (string.Equals(key, "owin.RequestQueryString", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x20u;
                        RequestQueryString = (string)value;
                        return true;
                    }
                   break;
                case 19:
                    if (string.Equals(key, "owin.RequestHeaders", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x40u;
                        RequestHeaders = (IDictionary<string, string[]>)value;
                        return true;
                    }
                   break;
                case 17:
                    if (string.Equals(key, "host.CallDisposed", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x100u;
                        CallDisposed = (CancellationToken)value;
                        return true;
                    }
                   break;
                case 33:
                    if (string.Equals(key, "System.Web.Routing.RequestContext", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x400u;
                        RequestContext = (RequestContext)value;
                        return true;
                    }
                   break;
                case 26:
                    if (string.Equals(key, "System.Web.HttpContextBase", StringComparison.Ordinal)) 
                    {
                        _flag0 |= 0x800u;
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
                    if (((_flag0 & 0x2u) != 0) && string.Equals(key, "owin.RequestMethod", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x2u;
                        RequestMethod = default(string);
                        return true;
                    }
                    if (((_flag0 & 0x4u) != 0) && string.Equals(key, "owin.RequestScheme", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x4u;
                        RequestScheme = default(string);
                        return true;
                    }
                   break;
                case 20:
                    if (((_flag0 & 0x8u) != 0) && string.Equals(key, "owin.RequestPathBase", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x8u;
                        RequestPathBase = default(string);
                        return true;
                    }
                   break;
                case 16:
                    if (((_flag0 & 0x10u) != 0) && string.Equals(key, "owin.RequestPath", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x10u;
                        RequestPath = default(string);
                        return true;
                    }
                    if (((_flag0 & 0x80u) != 0) && string.Equals(key, "owin.RequestBody", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x80u;
                        RequestBody = default(object);
                        return true;
                    }
                    if (((_flag0 & 0x200u) != 0) && string.Equals(key, "host.TraceOutput", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x200u;
                        TraceOutput = default(TextWriter);
                        return true;
                    }
                   break;
                case 23:
                    if (((_flag0 & 0x20u) != 0) && string.Equals(key, "owin.RequestQueryString", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x20u;
                        RequestQueryString = default(string);
                        return true;
                    }
                   break;
                case 19:
                    if (((_flag0 & 0x40u) != 0) && string.Equals(key, "owin.RequestHeaders", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x40u;
                        RequestHeaders = default(IDictionary<string, string[]>);
                        return true;
                    }
                   break;
                case 17:
                    if (((_flag0 & 0x100u) != 0) && string.Equals(key, "host.CallDisposed", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x100u;
                        CallDisposed = default(CancellationToken);
                        return true;
                    }
                   break;
                case 33:
                    if (((_flag0 & 0x400u) != 0) && string.Equals(key, "System.Web.Routing.RequestContext", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x400u;
                        RequestContext = default(RequestContext);
                        return true;
                    }
                   break;
                case 26:
                    if (((_flag0 & 0x800u) != 0) && string.Equals(key, "System.Web.HttpContextBase", StringComparison.Ordinal)) 
                    {
                        _flag0 &= ~0x800u;
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
                yield return "owin.RequestMethod";
            }
            if (((_flag0 & 0x4u) != 0))
            {
                yield return "owin.RequestScheme";
            }
            if (((_flag0 & 0x8u) != 0))
            {
                yield return "owin.RequestPathBase";
            }
            if (((_flag0 & 0x10u) != 0))
            {
                yield return "owin.RequestPath";
            }
            if (((_flag0 & 0x20u) != 0))
            {
                yield return "owin.RequestQueryString";
            }
            if (((_flag0 & 0x40u) != 0))
            {
                yield return "owin.RequestHeaders";
            }
            if (((_flag0 & 0x80u) != 0))
            {
                yield return "owin.RequestBody";
            }
            if (((_flag0 & 0x100u) != 0))
            {
                yield return "host.CallDisposed";
            }
            if (((_flag0 & 0x200u) != 0))
            {
                yield return "host.TraceOutput";
            }
            if (((_flag0 & 0x400u) != 0))
            {
                yield return "System.Web.Routing.RequestContext";
            }
            if (((_flag0 & 0x800u) != 0))
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
                yield return RequestMethod;
            }
            if (((_flag0 & 0x4u) != 0))
            {
                yield return RequestScheme;
            }
            if (((_flag0 & 0x8u) != 0))
            {
                yield return RequestPathBase;
            }
            if (((_flag0 & 0x10u) != 0))
            {
                yield return RequestPath;
            }
            if (((_flag0 & 0x20u) != 0))
            {
                yield return RequestQueryString;
            }
            if (((_flag0 & 0x40u) != 0))
            {
                yield return RequestHeaders;
            }
            if (((_flag0 & 0x80u) != 0))
            {
                yield return RequestBody;
            }
            if (((_flag0 & 0x100u) != 0))
            {
                yield return CallDisposed;
            }
            if (((_flag0 & 0x200u) != 0))
            {
                yield return TraceOutput;
            }
            if (((_flag0 & 0x400u) != 0))
            {
                yield return RequestContext;
            }
            if (((_flag0 & 0x800u) != 0))
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
                yield return new KeyValuePair<string,object>("owin.RequestMethod", RequestMethod);
            }
            if (((_flag0 & 0x4u) != 0))
            {
                yield return new KeyValuePair<string,object>("owin.RequestScheme", RequestScheme);
            }
            if (((_flag0 & 0x8u) != 0))
            {
                yield return new KeyValuePair<string,object>("owin.RequestPathBase", RequestPathBase);
            }
            if (((_flag0 & 0x10u) != 0))
            {
                yield return new KeyValuePair<string,object>("owin.RequestPath", RequestPath);
            }
            if (((_flag0 & 0x20u) != 0))
            {
                yield return new KeyValuePair<string,object>("owin.RequestQueryString", RequestQueryString);
            }
            if (((_flag0 & 0x40u) != 0))
            {
                yield return new KeyValuePair<string,object>("owin.RequestHeaders", RequestHeaders);
            }
            if (((_flag0 & 0x80u) != 0))
            {
                yield return new KeyValuePair<string,object>("owin.RequestBody", RequestBody);
            }
            if (((_flag0 & 0x100u) != 0))
            {
                yield return new KeyValuePair<string,object>("host.CallDisposed", CallDisposed);
            }
            if (((_flag0 & 0x200u) != 0))
            {
                yield return new KeyValuePair<string,object>("host.TraceOutput", TraceOutput);
            }
            if (((_flag0 & 0x400u) != 0))
            {
                yield return new KeyValuePair<string,object>("System.Web.Routing.RequestContext", RequestContext);
            }
            if (((_flag0 & 0x800u) != 0))
            {
                yield return new KeyValuePair<string,object>("System.Web.HttpContextBase", HttpContextBase);
            }
        }
	}
}
