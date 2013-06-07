// <copyright file="AuthenticationManager.cs" company="Microsoft Open Technologies, Inc.">
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

#if NET45

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Microsoft.Owin.Security
{
    class AuthenticationManager : IAuthenticationManager
    {
        private OwinRequest _request;
        private OwinResponse _response;

        public AuthenticationManager(OwinRequest request)
        {
            _request = request;
            _response = new OwinResponse(request);
        }

        public ClaimsPrincipal User
        {
            get { return _request.User as ClaimsPrincipal ?? new ClaimsPrincipal(_request.User); }
        }

        public IEnumerable<AuthenticationDescription> AuthenticationTypes()
        {
            // TODO: refactor the signature to remove the .Wait() on this call path
            var descriptions = new List<AuthenticationDescription>();
            _request.GetAuthenticationTypes((description, state) => ((List<AuthenticationDescription>)state).Add(new AuthenticationDescription(description)), descriptions).Wait();
            return descriptions;
        }

        public async Task<AuthenticateResult> AuthenticateAsync(string authenticationType)
        {
            return (await AuthenticateAsync(new[] { authenticationType })).SingleOrDefault();
        }

        public async Task<IEnumerable<AuthenticateResult>> AuthenticateAsync(string[] authenticationTypes)
        {
            var descriptions = new List<AuthenticateResult>();
            await _request.Authenticate(
                authenticationTypes,
                (identity, extra, description, state) => ((List<AuthenticateResult>)state).Add(new AuthenticateResult(identity, extra, description)), descriptions);
            return descriptions;
        }

        public void Challenge(string[] authenticationTypes, AuthenticationExtra extra)
        {
            _response.Challenge(authenticationTypes, extra);
        }

        public void SignIn(ClaimsPrincipal user, AuthenticationExtra extra)
        {
            _response.Grant(user, extra);
        }

        public void SignOut(string[] authenticationTypes)
        {
            _response.Revoke(authenticationTypes);
        }
    }
}

#endif
