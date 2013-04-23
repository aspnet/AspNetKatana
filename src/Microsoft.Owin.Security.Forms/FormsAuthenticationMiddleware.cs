// <copyright file="FormsAuthenticationMiddleware.cs" company="Microsoft Open Technologies, Inc.">
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
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.ModelSerializer;
using Microsoft.Owin.Security.TextEncoding;

namespace Microsoft.Owin.Security.Forms
{
    public class FormsAuthenticationMiddleware : AuthenticationMiddleware<FormsAuthenticationOptions>
    {
        private readonly IProtectionHandler<AuthenticationData> _modelProtection;

        public FormsAuthenticationMiddleware(OwinMiddleware next, FormsAuthenticationOptions options)
            : base(next, options)
        {
            if (options.Provider == null)
            {
                options.Provider = new FormsAuthenticationProvider();
            }
            var dataProtection = options.DataProtection;
            if (dataProtection == null)
            {
                dataProtection = DataProtectionProviders.Default.Create(
                    "FormsAuthenticationMiddleware",
                    options.AuthenticationType);
            }
            _modelProtection = new ProtectionHandler<AuthenticationData>(
                ModelSerializers.Ticket,
                dataProtection,
                TextEncodings.Base64Url);
        }

        protected override AuthenticationHandler<FormsAuthenticationOptions> CreateHandler()
        {
            return new FormsAuthenticationHandler(_modelProtection);
        }
    }
}
