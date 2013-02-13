using System.Collections.Generic;
using System.IO;
using System.Text;
using Owin.Types;

namespace Microsoft.AspNet.Razor.Owin.Execution
{
    public class RazorResponse : IRazorResponse
    {
        private OwinResponse _response;

        public RazorResponse(IDictionary<string,object> environment)
        {
            _response = new OwinResponse(environment);
        }

        public int StatusCode
        {
            get { return _response.StatusCode; }
            set { _response.StatusCode = value; }
        }

        public string ReasonPhrase
        {
            get { return _response.ReasonPhrase; }
            set { _response.ReasonPhrase = value; }
        }

        public Encoding Encoding { get; set; }

        public Stream Body
        {
            get { return _response.Body; }
            set { _response.Body = value; }
        }
    }
}