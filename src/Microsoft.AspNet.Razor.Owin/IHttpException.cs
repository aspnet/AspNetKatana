// -----------------------------------------------------------------------
// <copyright file="IHttpException.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Razor.Owin
{
    public interface IHttpException
    {
        int StatusCode { get; }
        string ReasonPhrase { get; }
    }
}
