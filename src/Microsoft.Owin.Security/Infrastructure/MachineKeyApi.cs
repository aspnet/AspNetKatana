// <copyright file="MachineKeyApi.cs" company="Microsoft Open Technologies, Inc.">
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

using System.Web.Security;

namespace Microsoft.Owin.Security.Infrastructure
{
    internal static class MachineKeyApi
    {
        private static readonly IApi Call = new Api();

        public interface IApi
        {
            byte[] Protect(byte[] userData, string[] purposes);
            byte[] Unprotect(byte[] protectedData, string[] purposes);
        }

        public static byte[] Protect(byte[] userData, string[] purposes)
        {
            return Call.Protect(userData, purposes);
        }

        public static byte[] Unprotect(byte[] protectedData, string[] purposes)
        {
            return Call.Unprotect(protectedData, purposes);
        }

        public class Api : IApi
        {
            public byte[] Protect(byte[] userData, string[] purposes)
            {
                return MachineKey.Protect(userData, purposes);
            }

            public byte[] Unprotect(byte[] protectedData, string[] purposes)
            {
                return MachineKey.Unprotect(protectedData, purposes);
            }
        }
    }
}
