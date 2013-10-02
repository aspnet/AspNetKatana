// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
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
        /// Defaults to all request paths in the current physical directory
        /// </summary>
        /// <param name="sharedOptions"></param>
        public StaticFileOptions(SharedOptions sharedOptions) : base(sharedOptions)
        {
            ContentTypeProvider = new FileExtensionContentTypeProvider();
            HeadersToSet = HeaderFields.ETag | HeaderFields.LastModified;
            ExpiresIn = TimeSpan.FromDays(1);
        }

        /// <summary>
        /// Used to map files to content-types.
        /// </summary>
        public IContentTypeProvider ContentTypeProvider { get; set; }

        /// <summary>
        /// The default content type for a request if the ContentTypeProvider cannot determine one.
        /// None is provided by default, so the client must determine the format themselves.
        /// http://www.w3.org/Protocols/rfc2616/rfc2616-sec7.html#sec7
        /// </summary>
        public string DefaultContentType { get; set; }

        /// <summary>
        /// If the file is not a recognized content-type should it be served?
        /// Default: false.
        /// </summary>
        public bool ServeUnknownFileTypes { get; set; }

        /// <summary>
        /// Specifies which response headers will be sent.  E-tag and Last-Modified are the default.
        /// </summary>
        public HeaderFields HeadersToSet { get; set; }

        /// <summary>
        /// Specifies an offset from the request date and time used to generate an date and time for
        /// the Expires header. A TimeSpan of zero will expire immediately.  TimeSpans should not
        /// exceed one year. HeadersToSet must also include the Expires header.
        /// </summary>
        public TimeSpan ExpiresIn { get; set; }

        /// <summary>
        /// Specifies a Cache-Control header on all responses.  There is no value by default.
        /// HeadersToSet must also include the Cache-Control header.
        /// </summary>
        public string CacheControl { get; set; }

        /// <summary>
        /// Invoked on each request to determine if the identified file should be served.
        /// All files are served if this is null.
        /// </summary>
        public IFileAccessPolicy AccessPolicy { get; set; }

        /// <summary>
        /// Sets the ContentTypeProvider.
        /// </summary>
        /// <param name="contentTypeProvider"></param>
        /// <returns>this</returns>
        public StaticFileOptions WithContentTypeProvider(IContentTypeProvider contentTypeProvider)
        {
            ContentTypeProvider = contentTypeProvider;
            return this;
        }

        /// <summary>
        /// Sets the DefaultContentType.
        /// </summary>
        /// <param name="defaultContentType"></param>
        /// <returns>this</returns>
        public StaticFileOptions WithDefaultContentType(string defaultContentType)
        {
            DefaultContentType = defaultContentType;
            return this;
        }

        /// <summary>
        /// Sets ServeUnknownFileTypes to true.
        /// </summary>
        /// <returns>this</returns>
        public StaticFileOptions WithServeUnknownFileTypes()
        {
            ServeUnknownFileTypes = true;
            return this;
        }

        internal bool ShouldSet(HeaderFields field)
        {
            return (HeadersToSet & field) == field;
        }
    }
}
