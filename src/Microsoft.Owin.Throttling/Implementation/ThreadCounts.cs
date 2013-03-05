// <copyright file="ThreadCounts.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

namespace Microsoft.Owin.Throttling.Implementation
{
    public struct ThreadCounts
    {
        public static ThreadCounts Zero = new ThreadCounts(0, 0);

        public ThreadCounts(int workerThreads, int completionPortThreads)
            : this()
        {
            WorkerThreads = workerThreads;
            CompletionPortThreads = completionPortThreads;
        }

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
