// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Owin.StaticFiles.DirectoryFormatters
{
    // Parses out the Accept header, sorts it, and finds the best match among the given list of formatters.
    internal class AcceptHeaderDirectoryFormatSelector : IDirectoryFormatSelector
    {
        internal AcceptHeaderDirectoryFormatSelector()
        {
        }

        // Reads the accept header and selects the most appropriate supported content-type
        // TODO: Consider separating out the accept header parsing into a stand-alone library.
        // e.g. System.Net.Http.Headers.HttpRequestHeaders.Accept.TryParseAdd.
        public bool TryDetermineFormatter(IOwinContext context, IList<IDirectoryInfoFormatter> formatters, out IDirectoryInfoFormatter formatter)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (formatters == null)
            {
                throw new ArgumentNullException("formatters");
            }

            // TODO:
            // Parse the Accept header

            string[] acceptHeaders;
            if (!context.Request.Headers.TryGetValue(Constants.Accept, out acceptHeaders)
                || acceptHeaders == null || acceptHeaders.Length == 0)
            {
                // RFC 2616 section 14.1: If no Accept header is present, then it is assumed that the client accepts all media types.
                // Send our fanciest version.
                formatter = formatters.First();
                return true;
            }

            string acceptHeader = acceptHeaders[0] ?? string.Empty;
            if (acceptHeaders.Length > 1)
            {
                acceptHeader = string.Join(", ", acceptHeaders);
            }

            // TODO: Split by ",", trim, split by ";", trim, sort by q value and our priorities
            // acceptHeaders = acceptHeader.Split(",");

            // Check for supported types:
            // text/plain
            // text/html
            // application/json
            // text/xml or application/xml?
            // */*?
            // text/*?
            for (int i = 0; i < formatters.Count; i++)
            {
                if (acceptHeader.Contains(formatters[i].ContentType))
                {
                    formatter = formatters[i];
                    return true;
                }
            }

            if (acceptHeader.Contains(Constants.AnyType))
            {
                formatter = formatters.First();
                return true;
            }

            formatter = null;
            return false;
        }
    }
}
