// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
