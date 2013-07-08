// <copyright file="GoogleAuthenticationOptions.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using Microsoft.Owin.Security.DataProtection;

namespace Microsoft.Owin.Security.Google
{
    public class GoogleAuthenticationOptions : AuthenticationOptions
    {
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", 
            MessageId = "Microsoft.Owin.Security.Google.GoogleAuthenticationOptions.set_Caption(System.String)", Justification = "Not localizable")]
        public GoogleAuthenticationOptions()
            : base(Constants.DefaultAuthenticationType)
        {
            Caption = Constants.DefaultAuthenticationType;
            ReturnEndpointPath = "/signin-google";
            AuthenticationMode = AuthenticationMode.Passive;
            BackchannelTimeout = 60 * 1000; // 60 seconds
        }

        /// <summary>
        /// Gets or sets the a pinned certificate validator to use to validate the endpoints used
        /// in back channel communications belong to Google.
        /// </summary>
        /// <value>
        /// The pinned certificate validator.
        /// </value>
        /// <remarks>If this property is null then the default certificate checks are performed,
        /// validating the subject name and if the signing chain is a trusted party.</remarks>
        public ICertificateValidator CertificateValidator { get; set; }

        /// <summary>
        /// Gets or sets timeout value in milliseconds for back channel communications with Twitter.
        /// </summary>
        /// <value>
        /// The back channel timeout in milliseconds.
        /// </value>
        public int BackchannelTimeout { get; set; }

        public string Caption
        {
            get { return Description.Caption; }
            set { Description.Caption = value; }
        }

        public string ReturnEndpointPath { get; set; }
        public string SignInAsAuthenticationType { get; set; }

        public IGoogleAuthenticationProvider Provider { get; set; }
        public ISecureDataFormat<AuthenticationExtra> StateDataFormat { get; set; }
    }
}
