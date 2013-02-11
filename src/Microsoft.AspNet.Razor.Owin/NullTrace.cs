// <copyright file="NullTrace.cs" company="Microsoft Open Technologies, Inc.">
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

namespace Microsoft.AspNet.Razor.Owin
{
    public class NullTrace : ITrace
    {
        public static readonly NullTrace Instance = new NullTrace();

        private NullTrace()
        {
        }

        public void WriteLine(string format, params object[] args)
        {
        }

        public IDisposable StartTrace()
        {
            return new DisposableAction(() => { });
        }
    }
}
