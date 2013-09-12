// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Owin.StaticFiles.ContentTypes;
using Microsoft.Owin.StaticFiles.Infrastructure;

namespace Microsoft.Owin.StaticFiles
{
    /// <summary>
    /// Options for serving static files
    /// </summary>
    public class StaticFileOptions : SharedOptionsBase<StaticFileOptions>
    {
        /// <summary>
        /// Defaults to all request paths in the current physical directory
        /// </summary>
        public StaticFileOptions() : this(new SharedOptions())
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sharedOptions"></param>
        public StaticFileOptions(SharedOptions sharedOptions) : base(sharedOptions)
        {
            ContentTypeProvider = new DefaultContentTypeProvider();
        }

        /// <summary>
        ///
        /// </summary>
        public IContentTypeProvider ContentTypeProvider { get; set; }

        /// <summary>
        /// The default content type for a request if the ContentTypeProvider cannot determine one.
        /// None is provided by default, so the client must guess.
        /// http://www.w3.org/Protocols/rfc2616/rfc2616-sec7.html#sec7
        /// </summary>
        public string DefaultContentType { get; set; }

        /// <summary>
        /// If the file is not a recognized content-type should it be served?
        /// Default: false.
        /// </summary>
        public bool ServeUnknownFileTypes { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentTypeProvider"></param>
        /// <returns>this</returns>
        public StaticFileOptions WithContentTypeProvider(IContentTypeProvider contentTypeProvider)
        {
            ContentTypeProvider = contentTypeProvider;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="defaultContentType"></param>
        /// <returns>this</returns>
        public StaticFileOptions WithDefaultContentType(string defaultContentType)
        {
            DefaultContentType = defaultContentType;
            return this;
        }
    }
}
