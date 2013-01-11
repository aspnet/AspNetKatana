// -----------------------------------------------------------------------
// <copyright file="CompilationMessage.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.AspNet.Razor.Owin.Compilation
{
    [Serializable]
    public class CompilationMessage
    {
        public CompilationMessage(MessageLevel level, string message) : this(level, message, null)
        {
        }

        public CompilationMessage(MessageLevel level, string message, FileLocation location)
        {
            Level = level;
            Message = message;
            Location = location;
        }

        public MessageLevel Level { get; private set; }
        public FileLocation Location { get; private set; }
        public string Message { get; private set; }
        
        public override string ToString()
        {
            return String.Format("[{0}]{1} - {2}",
                Level,
                Location == null ? String.Empty : (" (" + Location.ToString() + ")"),
                Message);
        }
    }
}
