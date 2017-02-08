// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;

namespace Microsoft.Owin.Hosting.Tracing
{
    /// <summary>
    /// Opens a stream writer for the given file.
    /// </summary>
    public class TraceOutputFactory : ITraceOutputFactory
    {
        /// <summary>
        /// Opens a stream writer for the given file.
        /// </summary>
        /// <param name="outputFile"></param>
        /// <returns></returns>
        public virtual TextWriter Create(string outputFile)
        {
            return string.IsNullOrWhiteSpace(outputFile)
                ? (TextWriter)new DualWriter(Console.Error)
                : new StreamWriter(outputFile, true);
        }

        // Writes to Debug and the given text writer
    }
}
