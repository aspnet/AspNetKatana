// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Owin.Diagnostics.Views;

namespace Microsoft.Owin.Diagnostics.Views
{
    /// <summary>
    /// Contains details for individual exception messages.
    /// </summary>
    public class ErrorDetails
    {
        /// <summary>
        /// An individual exception
        /// </summary>
        public Exception Error { get; set; }

        /// <summary>
        /// The generated stack frames
        /// </summary>
        public IEnumerable<StackFrame> StackFrames { get; set; }
    }
}
