// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NET40

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Owin.Security
{
    /// <summary>
    /// 
    /// </summary>
    public class AuthenticationDescription
    {
        private const string CaptionPropertyKey = "Caption";
        private const string AuthenticationTypePropertyKey = "AuthenticationType";

        /// <summary>
        /// 
        /// </summary>
        public AuthenticationDescription()
        {
            Properties = new Dictionary<string, object>(StringComparer.Ordinal);
        }

        /// <summary>
        /// 
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
        /// 
        /// </summary>
        public IDictionary<string, object> Properties { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string AuthenticationType
        {
            get { return GetString(AuthenticationTypePropertyKey); }
            set { Properties[AuthenticationTypePropertyKey] = value; }
        }

        /// <summary>
        /// 
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

#else

using ResharperCodeFormattingWorkaround = System.Object;

#endif
