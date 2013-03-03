namespace Microsoft.Owin.Throttling.Implementation
{
    public struct ThreadCounts
    {
        public ThreadCounts(int workerThreads, int completionPortThreads)
            : this()
        {
            WorkerThreads = workerThreads;
            CompletionPortThreads = completionPortThreads;
        }

        public static ThreadCounts Zero = new ThreadCounts(0, 0);

        public int WorkerThreads { get; set; }
        public int CompletionPortThreads { get; set; }

        public ThreadCounts Subtract(ThreadCounts counts)
        {
            return new ThreadCounts(WorkerThreads - counts.WorkerThreads, CompletionPortThreads - counts.CompletionPortThreads);
        }

        public int Greatest()
        {
            return WorkerThreads > CompletionPortThreads ? WorkerThreads : CompletionPortThreads;
        }
    }
}