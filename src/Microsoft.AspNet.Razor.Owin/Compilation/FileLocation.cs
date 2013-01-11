// -----------------------------------------------------------------------
// <copyright file="FileLocation.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
