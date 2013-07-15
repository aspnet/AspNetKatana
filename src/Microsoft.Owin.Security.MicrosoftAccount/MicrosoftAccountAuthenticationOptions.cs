// <copyright file="MicrosoftAccountAuthenticationOptions.cs" company="Microsoft Open Technologies, Inc.">
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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

namespace Microsoft.Owin.Security.MicrosoftAccount
{
    public class MicrosoftAccountAuthenticationOptions : AuthenticationOptions
    {
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", 
            MessageId = "Microsoft.Owin.Security.MicrosoftAccount.MicrosoftAccountAuthenticationOptions.set_Caption(System.String)", Justification = "Not localizable")]
        public MicrosoftAccountAuthenticationOptions() : base(Constants.DefaultAuthenticationType)
        {
            Caption = Constants.DefaultAuthenticationType;
            ReturnEndpointPath = "/signin-microsoft";
            AuthenticationMode = AuthenticationMode.Passive;
            Scope = new List<string> { "wl.basic" };
            BackchannelTimeout = TimeSpan.FromSeconds(60);
        }

        /// <summary>
        /// Gets or sets the a pinned certificate validator to use to validate the endpoints used
        /// in back channel communications belong to Microsoft Account.
        /// </summary>
        /// <value>
        /// The pinned certificate validator.
        /// </value>
        /// <remarks>If this property is null then the default certificate checks are performed,
        /// validating the subject name and if the signing chain is a trusted party.</remarks>
        public ICertificateValidator CertificateValidator { get; set; }

        public string Caption
        {
            get { return Description.Caption; }
            set { Description.Caption = value; }
        }

        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets timeout value in milliseconds for back channel communications with Twitter.
        /// </summary>
        /// <value>
        /// The back channel timeout.
        /// </value>
        public TimeSpan BackchannelTimeout { get; set; }

        /// <summary>
        /// The HttpMessageHandler used to communicate with Microsoft.
        /// This cannot be set at the same time as CertificateValidator unless the value 
        /// can be downcast to a WebRequestHandler.
        /// </summary>
        public HttpMessageHandler HttpHandler { get; set; }

        public IList<string> Scope { get; private set; }

        public string ReturnEndpointPath { get; set; }
        public string SignInAsAuthenticationType { get; set; }

        public IMicrosoftAccountAuthenticationProvider Provider { get; set; }
        public ISecureDataFormat<AuthenticationExtra> StateDataFormat { get; set; }

        internal void ResolveHttpMessageHandler()
        {
            HttpHandler = HttpHandler ?? new WebRequestHandler();

            // If they provided a validator, apply it or fail.
            if (CertificateValidator != null)
            {
                // Set the cert validate callback
                WebRequestHandler webRequestHandler = HttpHandler as WebRequestHandler;
                if (webRequestHandler == null)
                {
                    throw new InvalidOperationException(Resources.Exception_ValidatorHandlerMismatch);
                }
                webRequestHandler.ServerCertificateValidationCallback = CertificateValidator.Validate;
            }
        }
    }
}
