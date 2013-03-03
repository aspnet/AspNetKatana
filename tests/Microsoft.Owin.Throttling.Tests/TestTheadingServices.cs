using Microsoft.Owin.Throttling.Implementation;

namespace Microsoft.Owin.Throttling.Tests
{
    public class TestTheadingServices : IThreadingServices
    {
        public TestTheadingServices()
        {
            MaxThreads = new ThreadCounts(short.MaxValue, 1000);
            MinThreads = new ThreadCounts(8, 4);
            AvailableThreads = MaxThreads;
        }

        public ThreadCounts MaxThreads { get; set; }
        public ThreadCounts MinThreads { get; set; }
        public ThreadCounts AvailableThreads { get; set; }

        public ThreadCounts GetMaxThreads()
        {
            return MaxThreads;
        }

        public ThreadCounts GetMinThreads()
        {
            return MinThreads;
        }

        public ThreadCounts GetAvailableThreads()
        {
            return AvailableThreads;
        }

    }
}