// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Owin.Security.OAuth
{
    public class ClientDetails
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "To be compared to querystring value")]
        public string RedirectUri { get; set; }
    }
}
