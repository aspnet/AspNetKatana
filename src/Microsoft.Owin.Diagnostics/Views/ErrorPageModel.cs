// <copyright file="ErrorPageModel.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Collections.Generic;

namespace Microsoft.Owin.Diagnostics.Views
{
    /// <summary>
    /// Holds data to be displayed on the error page.
    /// </summary>
    public class ErrorPageModel
    {
        /// <summary>
        /// Options for what output to display.
        /// </summary>
        public ErrorPageOptions Options { get; set; }

        /// <summary>
        /// Detailed information about each exception in the stack
        /// </summary>
        public IEnumerable<ErrorDetails> ErrorDetails { get; set; }

        /// <summary>
        /// Parsed query data
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Model class contains collection")]
        public IReadableStringCollection Query { get; set; }

        // public IDictionary<string, string[]> Form { get; set; }

        /// <summary>
        /// Request cookies
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Model class contains collection")]
        public RequestCookieCollection Cookies { get; set; }

        /// <summary>
        /// Request headers
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Model class contains collection")]
        public IDictionary<string, string[]> Headers { get; set; }

        /// <summary>
        /// The request environment
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Model class contains collection")]
        public IDictionary<string, object> Environment { get; set; }
    }
}
