// <copyright file="TwitterAuthenticationOptions.cs" company="Microsoft Open Technologies, Inc.">
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

using Microsoft.Owin.Security.DataProtection;

namespace Microsoft.Owin.Security.Twitter
{
    public class TwitterAuthenticationOptions : AuthenticationOptions
    {
        public TwitterAuthenticationOptions() : base("Twitter")
        {
            Caption = "Twitter";
            this.CallbackUrlPath = "/signin-twitter";
            AuthenticationMode = AuthenticationMode.Passive;
            this.TwitterRequestTimeout = 60 * 1000; // 60 seconds
        }

        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }

        public int TwitterRequestTimeout { get; set; }

        public string Caption
        {
            get { return Description.Caption; }
            set { Description.Caption = value; }
        }

        public string CallbackUrlPath { get; set; }
        public string SignInAsAuthenticationType { get; set; }

        public IDataProtecter DataProtection { get; set; }
        public ITwitterAuthenticationProvider Provider { get; set; }
    }
}
