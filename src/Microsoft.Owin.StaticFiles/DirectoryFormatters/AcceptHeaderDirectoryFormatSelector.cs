// -----------------------------------------------------------------------
// <copyright file="AcceptDirectoryFormatParser.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Owin.StaticFiles.DirectoryFormatters
{
    // Parses out the Accept header, sorts it, and finds the best match among the given list of formatters.
    internal class AcceptHeaderDirectoryFormatSelector : IDirectoryFormatSelector
    {
        internal AcceptHeaderDirectoryFormatSelector()
        {
            // Prioritized list
            Formatters = new IDirectoryInfoFormatter[]
            {
                new HtmlDirectoryFormatter(),
                new JsonDirectoryFormatter(),
                new PlainTextDirectoryFormatter(),
            };

            DefaultFormatter = Formatters[0];
        }

        private IList<IDirectoryInfoFormatter> Formatters { get; set; }

        private IDirectoryInfoFormatter DefaultFormatter { get; set; }

        // Reads the accept header and selects the most appropriate supported content-type
        // TODO: Consider separating out the accept header parsing into a stand-alone library.
        // e.g. System.Net.Http.Headers.HttpRequestHeaders.Accept.TryParseAdd.
        public bool TryDetermineFormatter(IDictionary<string, object> environment, out IDirectoryInfoFormatter formatter)
        {
            if (environment == null)
            {
                throw new ArgumentNullException("environment");
            }

            // TODO:
            // Parse the Accept header
            var requestHeaders = (IDictionary<string, string[]>)environment[Constants.RequestHeadersKey];

            string[] acceptHeaders;
            if (!requestHeaders.TryGetValue(Constants.Accept, out acceptHeaders)
                || acceptHeaders == null || acceptHeaders.Length == 0)
            {
                // RFC 2616 section 14.1: If no Accept header is present, then it is assumed that the client accepts all media types.
                // Send our fanciest version.
                formatter = DefaultFormatter;
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
            for (int i = 0; i < Formatters.Count; i++)
            {
                if (acceptHeader.Contains(Formatters[i].ContentType))
                {
                    formatter = Formatters[i];
                    return true;
                }
            }

            if (acceptHeader.Contains(Constants.AnyType))
            {
                formatter = DefaultFormatter;
                return true;
            }

            formatter = null;
            return false;
        }
    }
}
