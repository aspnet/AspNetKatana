using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Owin
{
    public static class OwinApplication
    {
        static Lazy<Func<IDictionary<string, object>, Task>> _instance = new Lazy<Func<IDictionary<string, object>, Task>>(OwinBuilder.Build);

        public static Func<IDictionary<string, object>, Task> Instance
        {
            get { return _instance.Value; }
            set { _instance = new Lazy<Func<IDictionary<string, object>, Task>>(() => value); }
        }

        public static Func<Func<IDictionary<string, object>, Task>> Accessor
        {
            get { return () => _instance.Value; }
            set { _instance = new Lazy<Func<IDictionary<string, object>, Task>>(value); }
        }
    }
}
