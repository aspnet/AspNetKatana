// <copyright file="IDataProtection.cs" company="Microsoft Open Technologies, Inc.">
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

namespace Microsoft.Owin.Security.DataProtection
{
    /// <summary>
    /// Service used to protect and unprotect data
    /// </summary>
    public interface IDataProtecter
    {
        /// <summary>
        /// Called to protect user data.
        /// </summary>
        /// <param name="userData">The original data that must be protected</param>
        /// <returns>A different byte array that may be unprotected or altered only by software that has access to 
        /// the an identical IDataProtection service.</returns>
        byte[] Protect(byte[] userData);

        /// <summary>
        /// Called to unprotect user data
        /// </summary>
        /// <param name="protectedData">The byte array returned by a call to Protect on an identical IDataProtection service.</param>
        /// <returns>The byte array identical to the original userData passed to Protect.</returns>
        byte[] Unprotect(byte[] protectedData);
    }
}
