// <copyright file="Constants.cs" company="Microsoft Open Technologies, Inc.">
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

namespace Microsoft.Owin.Infrastructure
{
    internal static class Constants
    {
        internal const string Https = "HTTPS";

        internal static class Headers
        {
            internal const string ContentType = "Content-Type";
            internal const string CacheControl = "Cache-Control";
            internal const string MediaType = "Media-Type";
            internal const string Accept = "Accept";
            internal const string Host = "Host";
            internal const string ETag = "E-Tag";
            internal const string Location = "Location";
            internal const string ContentLength = "Content-Length";
            internal const string SetCookie = "Set-Cookie";
            internal const string Expires = "Expires";
        }
    }
}
