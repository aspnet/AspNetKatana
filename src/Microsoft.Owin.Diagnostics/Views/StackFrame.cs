// <copyright file="StackFrame.cs" company="Microsoft Open Technologies, Inc.">
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

using System.Collections.Generic;

namespace Microsoft.Owin.Diagnostics.Views
{
    /// <summary>
    /// Detailed exception stack information used to generate a view
    /// </summary>
    public class StackFrame
    {
        /// <summary>
        /// Function containing instruction
        /// </summary>
        public string Function { get; set; }

        /// <summary>
        /// File containing the instruction
        /// </summary>
        public string File { get; set; }

        /// <summary>
        /// The line number of the instruction
        /// </summary>
        public int Line { get; set; }

        /// <summary>
        /// The line preceeding the frame line
        /// </summary>
        public int PreContextLine { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<string> PreContextCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ContextCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<string> PostContextCode { get; set; }
    }
}
