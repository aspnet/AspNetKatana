// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
