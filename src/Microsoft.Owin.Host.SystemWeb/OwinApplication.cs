// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Microsoft.Owin.Host.SystemWeb
{
    internal static class OwinApplication
    {
        private static Lazy<OwinAppContext> _instance = new Lazy<OwinAppContext>(OwinBuilder.Build);
        private static ShutdownDetector _detector;

        internal static OwinAppContext Instance
        {
            get { return _instance.Value; }
            set { _instance = new Lazy<OwinAppContext>(() => value); }
        }

        internal static Func<OwinAppContext> Accessor
        {
            get { return () => _instance.Value; }
            set { _instance = new Lazy<OwinAppContext>(value); }
        }

        internal static CancellationToken ShutdownToken
        {
            get { return LazyInitializer.EnsureInitialized(ref _detector, InitShutdownDetector).Token; }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "Only cleaned up on shutdown")]
        private static ShutdownDetector InitShutdownDetector()
        {
            var detector = new ShutdownDetector();
            detector.Initialize();
            return detector;
        }
    }
}
