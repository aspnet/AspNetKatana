// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Owin.Host.SystemWeb.Infrastructure
{
    internal interface ITraceFactory
    {
        ITrace Create(string name);
    }
}
