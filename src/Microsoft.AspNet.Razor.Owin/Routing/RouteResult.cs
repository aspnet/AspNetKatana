// -----------------------------------------------------------------------
// <copyright file="RouteResult.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNet.Razor.Owin.IO;

namespace Microsoft.AspNet.Razor.Owin.Routing
{
    public class RouteResult
    {
        private RouteResult(bool success, IFile file, string pathInfo)
        {
            Success = success;
            File = file;
            PathInfo = pathInfo;
        }

        public bool Success { get; private set; }
        public IFile File { get; private set; }
        public string PathInfo { get; private set; }

        public static RouteResult Failed()
        {
            return new RouteResult(false, null, null);
        }

        public static RouteResult Successful(IFile file, string pathInfo)
        {
            return new RouteResult(true, file, pathInfo);
        }
    }
}
