// <copyright file="RouteResult.cs" company="Microsoft Open Technologies, Inc.">
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

using Microsoft.Owin.FileSystems;

namespace Microsoft.AspNet.Razor.Owin.Routing
{
    public class RouteResult
    {
        private RouteResult(bool success, IFileInfo file, string pathInfo)
        {
            Success = success;
            File = file;
            PathInfo = pathInfo;
        }

        public bool Success { get; private set; }
        public IFileInfo File { get; private set; }
        public string PathInfo { get; private set; }

        public static RouteResult Failed()
        {
            return new RouteResult(false, null, null);
        }

        public static RouteResult Successful(IFileInfo file, string pathInfo)
        {
            return new RouteResult(true, file, pathInfo);
        }
    }
}
