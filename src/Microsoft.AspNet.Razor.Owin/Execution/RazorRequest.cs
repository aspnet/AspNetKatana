using System.Collections.Generic;
using System.IO;
using Owin.Types;

namespace Microsoft.AspNet.Razor.Owin.Execution
{
    public class RazorRequest : IRazorRequest
    {
        private OwinRequest _request;

        public RazorRequest(IDictionary<string, object> environment)
        {
            _request = new OwinRequest(environment);
        }

        public IDictionary<string, object> Environment
        {
            get { return _request.Dictionary; }
        }

        public string Method
        {
            get { return _request.Method; }
            set { _request.Method = value; }
        }

        public string Path
        {
            get { return _request.Path; }
        }

        public TextWriter TraceOutput
        {
            get { return _request.TraceOutput; }
        }
    }
}