// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Owin.Host.SystemWeb.Infrastructure
{
    internal static class TraceFactory
    {
        private static ITraceFactory _instance = new DefaultTraceFactory();

        public static ITraceFactory Instance
        {
            get { return _instance; }
            set { _instance = value; }
        }

        public static ITrace Create(string name)
        {
            return Instance.Create(name);
        }
    }
}
