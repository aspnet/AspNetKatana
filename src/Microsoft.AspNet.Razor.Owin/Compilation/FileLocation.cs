// <copyright file="FileLocation.cs" company="Katana contributors">
//   Copyright 2011-2013 Katana contributors
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

using System;

namespace Microsoft.AspNet.Razor.Owin.Compilation
{
    [Serializable]
    public class FileLocation
    {
        public FileLocation(string fileName)
        {
            FileName = fileName;
            LineNumber = null;
            Column = null;
        }

        public FileLocation(string fileName, int lineNumber, int column) : this(fileName)
        {
            LineNumber = lineNumber;
            Column = column;
        }

        public FileLocation(string fileName, int lineNumber, int column, bool inGeneratedCode)
            : this(fileName, lineNumber, column)
        {
            InGeneratedCode = inGeneratedCode;
        }

        public string FileName { get; private set; }
        public int? LineNumber { get; private set; }
        public int? Column { get; private set; }
        public bool InGeneratedCode { get; private set; }

        public override string ToString()
        {
            return String.Concat(
                InGeneratedCode ? "[Generated Source Code]" : FileName,
                LineNumber == null ? String.Empty :
                                                      String.Format(":{0},{1}", LineNumber.Value, Column.Value));
        }
    }
}
