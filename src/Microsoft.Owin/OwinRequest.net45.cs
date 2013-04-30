// <copyright file="OwinRequest.net45.cs" company="Microsoft Open Technologies, Inc.">
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

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Security.Principal;
using System.Threading.Tasks;
using Owin.Types.Helpers;

namespace Microsoft.Owin
{
    public partial struct OwinRequest
    {
        public async Task<NameValueCollection> ReadForm()
        {
            var form = new NameValueCollection();
            await ReadForm(form);
            return form;
        }

        public async Task ReadForm(Action<string, string, object> callback, object state)
        {
            using (var reader = new StreamReader(Body))
            {
                string text = await reader.ReadToEndAsync();
                OwinHelpers.ParseDelimited(text, new[] { '&' }, callback, state);
            }
        }

        public async Task ReadForm(NameValueCollection form)
        {
            using (var reader = new StreamReader(Body))
            {
                string text = await reader.ReadToEndAsync();
                OwinHelpers.ParseDelimited(text, new[] { '&' }, (name, value, state) => ((NameValueCollection)state).Add(name, value), form);
            }
        }

        public async Task ReadForm(IDictionary<string, string[]> form)
        {
            using (var reader = new StreamReader(Body))
            {
                string text = await reader.ReadToEndAsync();
                OwinHelpers.ParseDelimited(text, new[] { '&' }, (name, value, state) => ((IDictionary<string, string[]>)state).Add(name, new[] { value }), form);
            }
        }

        public async Task Authenticate(string[] authenticationTypes, Action<IIdentity, IDictionary<string, string>, IDictionary<string, object>, object> callback, object state)
        {
            throw new NotImplementedException();
        }

        public async Task GetAuthenticationTypes(Action<IDictionary<string, object>, object> callback, object state)
        {
            throw new NotImplementedException();
        }
    }
}

#endif
