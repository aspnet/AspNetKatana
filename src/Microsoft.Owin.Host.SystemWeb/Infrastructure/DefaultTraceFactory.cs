using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Microsoft.Owin.Host.SystemWeb.Infrastructure
{
    internal class DefaultTraceFactory : ITraceFactory
    {
        private readonly SourceSwitch _switch = new SourceSwitch("Microsoft.Owin.Host.SystemWeb");
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
            if (key == "Microsoft.Owin.Host.SystemWeb")
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
                    TraceSource rootSource = GetOrAddTraceSource("Microsoft.Owin.Host.SystemWeb");
                    traceSource.Listeners.Clear();
                    traceSource.Listeners.AddRange(rootSource.Listeners);
                }
                if (HasDefaultSwitch(traceSource))
                {
                    TraceSource rootSource = GetOrAddTraceSource("Microsoft.Owin.Host.SystemWeb");
                    traceSource.Switch = rootSource.Switch;
                }
            }

            return traceSource;
        }

        private bool HasDefaultListeners(TraceSource traceSource)
        {
            return traceSource.Listeners.Count == 1 && traceSource.Listeners[0] is DefaultTraceListener;
        }

        private bool HasDefaultSwitch(TraceSource traceSource)
        {
            return string.IsNullOrEmpty(traceSource.Switch.DisplayName) == string.IsNullOrEmpty(traceSource.Name) &&
                traceSource.Switch.Level == SourceLevels.Off;
        }
    }
}
