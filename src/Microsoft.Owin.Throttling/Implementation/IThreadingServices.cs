namespace Microsoft.Owin.Throttling.Implementation
{
    public interface IThreadingServices
    {
        ThreadCounts GetAvailableThreads();
        ThreadCounts GetMaxThreads();
    }
}