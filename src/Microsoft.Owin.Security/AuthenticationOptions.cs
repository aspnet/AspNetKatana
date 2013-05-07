// <copyright file="AuthenticationOptions.cs" company="Microsoft Open Technologies, Inc.">
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

namespace Microsoft.Owin.Security
{
    /// <summary>
    /// Base Options for all authentication middleware
    /// </summary>
    public abstract class AuthenticationOptions
    {
        private string _authenticationType;

        /// <summary>
        /// Initialize properties of AuthenticationOptions base class
        /// </summary>
        /// <param name="authenticationType">Assigned to the AuthenticationType property</param>
        protected AuthenticationOptions(string authenticationType)
        {
            Description = new AuthenticationDescription();
            AuthenticationType = authenticationType;
            AuthenticationMode = AuthenticationMode.Active;
        }

        /// <summary>
        /// The AuthenticationType in the options corresponds to the IIdentity AuthenticationType property. A different
        /// value may be assigned in order to use the same authentication middleware type more than once in a pipeline.
        /// </summary>
        public string AuthenticationType
        {
            get { return _authenticationType; }
            set
            {
                _authenticationType = value;
                Description.AuthenticationType = value;
            }
        }

        /// <summary>
        /// If Active the authentication middleware alter the request user coming in and
        /// alter 401 Unauthorized responses going out. If Passive the authentication middleware will only provide
        /// identity and alter responses when explicitly indicated by the AuthenticationType.
        /// </summary>
        public AuthenticationMode AuthenticationMode { get; set; }

        /// <summary>
        /// Additional information about the authentication type which is made available to the application.
        /// </summary>
        public AuthenticationDescription Description { get; set; }
    }
}
