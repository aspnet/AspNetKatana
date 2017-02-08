// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Owin.Hosting
{
    /// <summary>
    /// Settings to control the startup behavior of an OWIN application
    /// </summary>
    [Serializable]
    public class StartOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StartOptions"/> class
        /// </summary>
        public StartOptions()
        {
            Urls = new List<string>();
            // Web.Config appSettings are case-insensitive
            Settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StartOptions"/> class
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#", Justification = "May contain Uri invalid host characters")]
        public StartOptions(string url)
            : this()
        {
            Urls.Add(url);
        }

        /// <summary>
        /// A list of url prefixes to listen on. Overrides port.
        /// </summary>
        public IList<string> Urls { get; private set; }

        /// <summary>
        /// A port to listen on.
        /// </summary>
        public int? Port { get; set; }

        /// <summary>
        /// Parameter to locate and load web application startup routine
        /// </summary>
        public string AppStartup { get; set; }

        /// <summary>
        /// Name of the assembly containing the http server implementation
        /// </summary>
        public string ServerFactory { get; set; }

        /// <summary>
        /// Optional settings used to override service types and other defaults
        /// </summary>
        public IDictionary<string, string> Settings { get; private set; }
    }
}
