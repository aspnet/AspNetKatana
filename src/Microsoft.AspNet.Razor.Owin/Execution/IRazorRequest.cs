using System.Collections.Generic;
using System.IO;

namespace Microsoft.AspNet.Razor.Owin.Execution
{
    public interface IRazorRequest
    {
        IDictionary<string, object> Environment { get; }
        string Path { get; }
        TextWriter TraceOutput { get; }
    }
}