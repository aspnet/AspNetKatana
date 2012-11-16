using System.Diagnostics;

namespace Microsoft.Owin.Host.SystemWeb.Infrastructure
{
    internal interface ITrace
    {
        void Write(TraceEventType eventType, string format, params object[] args);
    }
}