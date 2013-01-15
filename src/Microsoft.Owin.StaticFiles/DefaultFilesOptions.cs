// -----------------------------------------------------------------------
// <copyright file="DefaultFilesOptions.cs" company="Katana contributors">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin.StaticFiles.Infrastructure;

namespace Microsoft.Owin.StaticFiles
{
    public class DefaultFilesOptions : SharedOptionsBase<DefaultFilesOptions>
    {
        public DefaultFilesOptions()
            : this(new SharedOptions())
        {
        }

        public DefaultFilesOptions(SharedOptions sharedOptions)
            : base(sharedOptions)
        {
            // Prioritized list
            DefaultFileNames = new List<string>()
            {
                "default.htm",
                "default.html",
                "index.htm",
                "index.html",
            };
        }

        public IList<string> DefaultFileNames { get; private set; }

        public DefaultFilesOptions WithDefaultFileNames(IEnumerable<string> defaultFileNames)
        {
            DefaultFileNames = defaultFileNames.ToList();
            return this;
        }
    }
}
