// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;
using Microsoft.Owin.FileSystems;

namespace Microsoft.Owin.StaticFiles.DirectoryFormatters
{
    internal class XmlDirectoryFormatter : IDirectoryInfoFormatter
    {
        public string ContentType
        {
            get { return Constants.TextXml; }
        }

        public StringBuilder GenerateContent(PathString requestPath, IEnumerable<IFileInfo> contents)
        {
            if (contents == null)
            {
                throw new ArgumentNullException("contents");
            }

            StringBuilder builder = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings() { Indent = true };
            using (XmlWriter writer = XmlWriter.Create(builder, settings))
            {
                writer.WriteStartDocument();

                writer.WriteStartElement("Files");
                {
                    foreach (IFileInfo file in contents)
                    {
                        if (file.IsDirectory)
                        {
                            writer.WriteStartElement("Directory");
                            writer.WriteAttributeString("Name", file.Name);
                            writer.WriteAttributeString("LastModified", 
                                file.LastModified.ToString(Constants.HttpDateFormat, CultureInfo.InvariantCulture));
                            writer.WriteEndElement();
                        }
                    }

                    foreach (IFileInfo file in contents)
                    {
                        if (!file.IsDirectory)
                        {
                            writer.WriteStartElement("File");
                            writer.WriteAttributeString("Name", file.Name);
                            writer.WriteAttributeString("Length", file.Length.ToString(CultureInfo.InvariantCulture));
                            writer.WriteAttributeString("LastModified", 
                                file.LastModified.ToString(Constants.HttpDateFormat, CultureInfo.InvariantCulture));
                            writer.WriteEndElement();
                        }
                    }
                }
                writer.WriteEndElement();
            }
            return builder;
        }
    }
}
