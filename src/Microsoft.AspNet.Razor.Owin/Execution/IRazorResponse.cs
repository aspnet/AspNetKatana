using System.IO;
using System.Text;

namespace Microsoft.AspNet.Razor.Owin.Execution
{
    public interface IRazorResponse
    {
        int StatusCode { get; set; }
        string ReasonPhrase { get; set; }
        Encoding Encoding { get; set; }
        Stream Body { get; set; }
    }
}