// -----------------------------------------------------------------------
// <copyright file="ErrorMessage.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNet.Razor.Owin.Compilation;

namespace Microsoft.AspNet.Razor.Owin
{
    public class ErrorMessage : IErrorMessage
    {
        public ErrorMessage(string message)
        {
            Message = message;
            Location = new FileLocation(null);
        }

        public ErrorMessage(string message, FileLocation location)
        {
            Message = message;
            Location = location;
        }

        public ErrorMessage(CompilationMessage cm)
        {
            Message = cm.Message;
            Location = cm.Location;
        }

        public FileLocation Location { get; set; }
        public string Message { get; set; }
    }
}
