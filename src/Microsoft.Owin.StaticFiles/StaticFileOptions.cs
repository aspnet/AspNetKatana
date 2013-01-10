// -----------------------------------------------------------------------
// <copyright file="StaticFileOptions.cs" company="Katana contributors">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin.StaticFiles.ContentTypes;
using Microsoft.Owin.StaticFiles.FileSystems;
using Microsoft.Owin.StaticFiles.Infrastructure;

namespace Microsoft.Owin.StaticFiles
{
    public class StaticFileOptions : SharedOptionsBase<StaticFileOptions>
    {
        public StaticFileOptions() : this(new SharedOptions())
        {
        }
  
        public StaticFileOptions(SharedOptions sharedOptions) : base(sharedOptions)
        {
            ContentTypeProvider = new DefaultContentTypeProvider();
        }

        public IContentTypeProvider ContentTypeProvider { get; set; }
        public string DefaultContentType { get; set; }


        public StaticFileOptions WithContentTypeProvider(IContentTypeProvider contentTypeProvider)
        {
            ContentTypeProvider = contentTypeProvider;
            return this;
        }

        public StaticFileOptions WithDefaultContentType(string defaultContentType)
        {
            DefaultContentType = defaultContentType;
            return this;
        }
    }
}
