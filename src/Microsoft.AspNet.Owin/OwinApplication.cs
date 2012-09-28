using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace Microsoft.AspNet.Owin
{
    public static class OwinApplication
    {
        static Lazy<Func<IDictionary<string, object>, Task>> _instance = new Lazy<Func<IDictionary<string, object>, Task>>(OwinBuilder.Build);
        static ShutdownDetector _detector;

        public static Func<IDictionary<string, object>, Task> Instance
        {
            get { return _instance.Value; }
            set { _instance = new Lazy<Func<IDictionary<string, object>, Task>>(() => value); }
        }

        public static Func<Func<IDictionary<string, object>, Task>> Accessor
        {
            get { return () => _instance.Value; }
            set { _instance = new Lazy<Func<IDictionary<string, object>, Task>>(value); }
        }

        public static CancellationToken ShutdownToken
        {
            get { return LazyInitializer.EnsureInitialized(ref _detector, InitShutdownDetector).Token; }
        }

        static ShutdownDetector InitShutdownDetector()
        {
            var detector = new ShutdownDetector();
            detector.Initialize();
            return detector;
        }

        class ShutdownDetector : IRegisteredObject
        {
            readonly CancellationTokenSource _cts = new CancellationTokenSource();

            public CancellationToken Token
            {
                get { return _cts.Token; }
            }

            public void Initialize()
            {
                try
                {
                    HostingEnvironment.RegisterObject(this);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            public void Stop(bool immediate)
            {
                try
                {
                    _cts.Cancel(throwOnFirstException: false);
                }
                catch
                {
                    // Swallow the exception as Stop should never throw
                    // TODO: Log exceptions
                }
                finally
                {
                    HostingEnvironment.UnregisterObject(this);
                }
            }
        }

    }
}
