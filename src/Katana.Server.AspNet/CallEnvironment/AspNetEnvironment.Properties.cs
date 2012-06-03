
using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;

namespace Katana.Server.AspNet.CallEnvironment
{
	public partial class AspNetEnvironment
	{
        public RequestContext RequestContext {get;set;}
        public HttpContextBase HttpContextBase {get;set;}

        bool PropertiesContainsKey(string key)
        {
            switch(key.Length)
            {
                case 33:
                    if (string.Equals(key, "System.Web.Routing.RequestContext", StringComparison.Ordinal)) 
                    {
                        return true;
                    }
                   break;
                case 26:
                    if (string.Equals(key, "System.Web.HttpContextBase", StringComparison.Ordinal)) 
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
                case 33:
                    if (string.Equals(key, "System.Web.Routing.RequestContext", StringComparison.Ordinal)) 
                    {
                        value = RequestContext;
                        return true;
                    }
                   break;
                case 26:
                    if (string.Equals(key, "System.Web.HttpContextBase", StringComparison.Ordinal)) 
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
                case 33:
                    if (string.Equals(key, "System.Web.Routing.RequestContext", StringComparison.Ordinal)) 
                    {
                        RequestContext = (RequestContext)value;
                        return true;
                    }
                   break;
                case 26:
                    if (string.Equals(key, "System.Web.HttpContextBase", StringComparison.Ordinal)) 
                    {
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
                case 33:
                    if (string.Equals(key, "System.Web.Routing.RequestContext", StringComparison.Ordinal)) 
                    {
                        RequestContext = null;
                        return true;
                    }
                   break;
                case 26:
                    if (string.Equals(key, "System.Web.HttpContextBase", StringComparison.Ordinal)) 
                    {
                        HttpContextBase = null;
                        return true;
                    }
                   break;
            }
            return false;
        }
        static string[] _knownKeys = new []
        {
            "System.Web.Routing.RequestContext",
            "System.Web.HttpContextBase",
        };

        IEnumerable<string> PropertiesKeys()
        {
            return _knownKeys;
        }

        ICollection<object> PropertiesValues()
        {
            return new object[]
            {
                RequestContext,
                HttpContextBase,
            };
        }
	}
}
