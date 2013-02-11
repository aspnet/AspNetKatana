// <copyright file="DefaultTraceFactory.cs" company="Microsoft Open Technologies, Inc.">
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

using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Microsoft.Owin.Host.SystemWeb.Infrastructure
{
    internal class DefaultTraceFactory : ITraceFactory
    {
        private const string RootTraceName = "Microsoft.Owin.Host.SystemWeb";

        private readonly SourceSwitch _switch = new SourceSwitch(RootTraceName);
        private readonly ConcurrentDictionary<string, TraceSource> _sources = new ConcurrentDictionary<string, TraceSource>(StringComparer.OrdinalIgnoreCase);

        public ITrace Create(string name)
        {
            return new DefaultTrace(GetOrAddTraceSource(name));
        }

        private TraceSource GetOrAddTraceSource(string name)
        {
            return _sources.GetOrAdd(name, InitializeTraceSource);
        }

        private TraceSource InitializeTraceSource(string key)
        {
            var traceSource = new TraceSource(key);
            if (key == RootTraceName)
            {
                if (HasDefaultSwitch(traceSource))
                {
                    traceSource.Switch = _switch;
                }
            }
            else
            {
                if (HasDefaultListeners(traceSource))
                {
                    TraceSource rootSource = GetOrAddTraceSource(RootTraceName);
                    traceSource.Listeners.Clear();
                    traceSource.Listeners.AddRange(rootSource.Listeners);
                }
                if (HasDefaultSwitch(traceSource))
                {
                    TraceSource rootSource = GetOrAddTraceSource(RootTraceName);
                    traceSource.Switch = rootSource.Switch;
                }
            }

            return traceSource;
        }

        private static bool HasDefaultListeners(TraceSource traceSource)
        {
            return traceSource.Listeners.Count == 1 && traceSource.Listeners[0] is DefaultTraceListener;
        }

        private static bool HasDefaultSwitch(TraceSource traceSource)
        {
            return string.IsNullOrEmpty(traceSource.Switch.DisplayName) == string.IsNullOrEmpty(traceSource.Name) &&
                traceSource.Switch.Level == SourceLevels.Off;
        }
    }
}
