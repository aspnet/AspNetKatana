// <copyright file="Encapsulate.cs" company="Katana contributors">
//   Copyright 2011-2013 Katana contributors
// </copyright>
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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Microsoft.Owin.Hosting.Utilities
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class Encapsulate
    {
        private readonly AppFunc _app;
        private readonly IList<KeyValuePair<string, object>> _environmentData;

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public Encapsulate(AppFunc app, IList<KeyValuePair<string, object>> environmentData)
        {
            _app = app;
            _environmentData = environmentData;
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            if (environment == null)
            {
                throw new ArgumentNullException("environment");
            }

            for (int i = 0; i < _environmentData.Count; i++)
            {
                KeyValuePair<string, object> pair = _environmentData[i];
                object obj;
                if (!environment.TryGetValue(pair.Key, out obj) || obj == null)
                {
                    environment[pair.Key] = pair.Value;
                }
            }

            return _app.Invoke(environment);
        }
    }
}
