// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if NET40
using ResharperCodeFormattingWorkaround = System.Object;

namespace System.Reflection
{
    /// <summary>
    /// Provided as a down-level stub for the 4.5 AssemblyMetaDataAttribute class.
    /// All released assemblies should define [AssemblyMetadata(“Servicing”,”true”)].
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    internal sealed class AssemblyMetadataAttribute : Attribute
    {
        public AssemblyMetadataAttribute(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; set; }
        public string Value { get; set; }
    }
}

#else

using ResharperCodeFormattingWorkaround = System.Object;

#endif
