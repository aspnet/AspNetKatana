// <copyright file="ICertificateValidator.cs" company="Microsoft Open Technologies, Inc.">
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

using System.Net.Security;

namespace Microsoft.Owin.Security
{
    /// <summary>
    /// Interface for providing pinned certificate validation, which checks HTTPS 
    /// communication against a known good list of certificates to protect against 
    /// compromised or rogue CAs issuing certificates for hosts without the 
    /// knowledge of the host owner.
    /// </summary>
    public interface ICertificateValidator
    {
        /// <summary>
        /// Gets the function used to validate HTTPS certificates.
        /// </summary>
        /// <value>
        /// The function used to validate HTTPS certificates.
        /// </value>
        RemoteCertificateValidationCallback RemoteCertificateValidationCallback
        {
            get;
        }
    }
}