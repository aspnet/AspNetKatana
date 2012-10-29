// <copyright file="Constants.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
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

namespace Microsoft.Owin.Host.SystemWeb
{
    /// <summary>
    /// Standard keys and values for use within the OWIN interfaces
    /// </summary>
    internal static class Constants
    {
        public const string ServerNameKey = "server.Name";
        public static readonly string ServerName = "ASP.NET 4.0, Microsoft.AspNet.Owin " + typeof(Constants).Assembly.GetName().Version.ToString();
        public const string ServerVersionKey = "msaspnet.AdapterVersion";
        public static readonly string ServerVersion = typeof(Constants).Assembly.GetName().Version.ToString();

        public const string ServerCapabilitiesKey = "server.Capabilities";

        public const string SendFileVersionKey = "sendfile.Version";
        public const string SendFileVersion = "1.0";

        public const string SendFileFuncKey = "sendfile.Func";
    }
}
