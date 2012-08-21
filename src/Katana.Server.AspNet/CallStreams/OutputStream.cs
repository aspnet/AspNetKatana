using System.IO;
using System.Web;

namespace Katana.Server.AspNet.CallStreams
{
    public class OutputStream : DelegatingStream
    {
        readonly HttpResponseBase _response;

        public OutputStream(HttpResponseBase response, Stream stream)
            : base(stream)
        {
            _response = response;
        }

        public override void Flush()
        {
            base.Flush();
            _response.Flush();
        }
    }
}
