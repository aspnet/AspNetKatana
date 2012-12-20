using System.IO;

namespace Microsoft.Owin.Hosting.Tracing
{
    public interface ITraceOutputBinder
    {
        TextWriter Create(string outputFileParameter);
    }
}