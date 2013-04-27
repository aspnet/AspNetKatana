// <copyright file="TextEncodings.cs" company="Microsoft Open Technologies, Inc.">
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

namespace Microsoft.Owin.Security.DataHandler.Encoder
{
    public static class TextEncodings
    {
        private static readonly ITextEncoder Base64Instance = new Base64TextEncoder();
        private static readonly ITextEncoder Base64UrlInstance = new Base64UrlTextEncoder();

        public static ITextEncoder Base64
        {
            get { return Base64Instance; }
        }

        public static ITextEncoder Base64Url
        {
            get { return Base64UrlInstance; }
        }
    }
}
