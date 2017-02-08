// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Owin.Security
{
    /// <summary>
    /// Contains information describing an authentication provider.
    /// </summary>
    public class AuthenticationDescription
    {
        private const string CaptionPropertyKey = "Caption";
        private const string AuthenticationTypePropertyKey = "AuthenticationType";

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationDescription"/> class
        /// </summary>
        public AuthenticationDescription()
        {
            Properties = new Dictionary<string, object>(StringComparer.Ordinal);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationDescription"/> class
        /// </summary>
        /// <param name="properties"></param>
        public AuthenticationDescription(IDictionary<string, object> properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException("properties");
            }
            Properties = properties;
        }

        /// <summary>
        /// Contains metadata about the authentication provider.
        /// </summary>
        public IDictionary<string, object> Properties { get; private set; }

        /// <summary>
        /// Gets or sets the name used to reference the authentication middleware instance.
        /// </summary>
        public string AuthenticationType
        {
            get { return GetString(AuthenticationTypePropertyKey); }
            set { Properties[AuthenticationTypePropertyKey] = value; }
        }

        /// <summary>
        /// Gets or sets the display name for the authentication provider.
        /// </summary>
        public string Caption
        {
            get { return GetString(CaptionPropertyKey); }
            set { Properties[CaptionPropertyKey] = value; }
        }

        private string GetString(string name)
        {
            object value;
            if (Properties.TryGetValue(name, out value))
            {
                return Convert.ToString(value, CultureInfo.InvariantCulture);
            }
            return null;
        }
    }
}
