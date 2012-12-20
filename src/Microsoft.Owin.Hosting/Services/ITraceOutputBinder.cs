using System.IO;

namespace Microsoft.Owin.Hosting.Services
{
    public interface ITraceOutputBinder
    {
        TextWriter Create(string outputFileParameter);
    }

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