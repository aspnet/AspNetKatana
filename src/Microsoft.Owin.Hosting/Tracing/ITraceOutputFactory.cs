// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;

namespace Microsoft.Owin.Hosting.Tracing
{
    /// <summary>
    /// Used to create the trace output.
    /// </summary>
    public interface ITraceOutputFactory
    {
        /// <summary>
        /// Used to create the trace output.
        /// </summary>
        /// <param name="outputFile"></param>
        /// <returns></returns>
        TextWriter Create(string outputFile);
    }
}
