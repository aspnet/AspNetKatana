using System.IO;

namespace Microsoft.Owin.Hosting.Tracing
{
    public class DefaultTraceOutputBinder : ITraceOutputBinder
    {
        public TextWriter Create(string outputFileParameter)
        {
            return string.IsNullOrWhiteSpace(outputFileParameter) 
                ? null 
                : new StreamWriter(outputFileParameter, true);
        }
    }
}