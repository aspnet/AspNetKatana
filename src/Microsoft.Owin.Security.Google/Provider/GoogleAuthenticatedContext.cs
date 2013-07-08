// <copyright file="GoogleAuthenticatedContext.cs" company="Microsoft Open Technologies, Inc.">
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

using System.Collections.Generic;
using System.Security.Claims;
using System.Xml.Linq;
using Microsoft.Owin.Security.Provider;

namespace Microsoft.Owin.Security.Google
{
    public class GoogleAuthenticatedContext : BaseContext
    {
        public GoogleAuthenticatedContext(
            IOwinContext context,
            ClaimsIdentity identity,
            AuthenticationExtra extra,
            XElement responseMessage,
            IDictionary<string, string> attributeExchangeProperties)
            : base(context)
        {
            Identity = identity;
            Extra = extra;
            ResponseMessage = responseMessage;
            AttributeExchangeProperties = attributeExchangeProperties;
        }

        public ClaimsIdentity Identity { get; set; }
        public AuthenticationExtra Extra { get; set; }

        public XElement ResponseMessage { get; set; }
        public IDictionary<string, string> AttributeExchangeProperties { get; private set; }
    }
}
