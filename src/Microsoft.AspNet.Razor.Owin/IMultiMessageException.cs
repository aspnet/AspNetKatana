// -----------------------------------------------------------------------
// <copyright file="IMultiMessageException.cs" company="Microsoft">
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
    public interface IMultiMessageException
    {
        string MessageListTitle { get; }
        IEnumerable<IErrorMessage> Messages { get; }
    }
}
