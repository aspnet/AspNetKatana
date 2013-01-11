// -----------------------------------------------------------------------
// <copyright file="PageBase.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Gate;
using Microsoft.AspNet.Razor.Owin;
using Microsoft.AspNet.Razor.Owin.Execution;

namespace Microsoft.AspNet.Razor.Owin
{
    public abstract class PageBase : IRazorPage
    {
        public Request Request { get; private set; }
        public Response Response { get; private set; }
        public TextWriter Output { get; private set; }

        public Task Run(Request req, Response resp)
        {
            Request = req;
            Response = resp;
            Output = new ResponseWriter(resp);
            Execute();
            return Task.FromResult(new object());
        }

        public abstract void Execute();

        public virtual void Write(object text)
        {
            WriteTo(Output, text);
        }

        public virtual void WriteTo(TextWriter writer, object text)
        {
            writer.Write(WebUtility.HtmlEncode(text.ToString()));
        }

        public virtual void WriteLiteral(object text)
        {
            WriteLiteralTo(Output, text);
        }

        public virtual void WriteLiteralTo(TextWriter writer, object text)
        {
            writer.Write(text.ToString());
        }

        public virtual void WriteAttribute(string name, PositionTagged<string> prefix, PositionTagged<string> suffix, params AttributeValue[] values)
        {
            WriteAttributeTo(Output, name, prefix, suffix, values);
        }

        public virtual string Href(string input)
        {
            return input;
        }

        protected internal virtual void WriteAttributeTo(TextWriter writer, string name, PositionTagged<string> prefix, PositionTagged<string> suffix, params AttributeValue[] values)
        {
            bool first = true;
            bool wroteSomething = false;
            if (values.Length == 0)
            {
                // Explicitly empty attribute, so write the prefix and suffix
                WritePositionTaggedLiteral(writer, prefix);
                WritePositionTaggedLiteral(writer, suffix);
            }
            else
            {
                for (int i = 0; i < values.Length; i++)
                {
                    AttributeValue attrVal = values[i];
                    PositionTagged<object> val = attrVal.Value;
                    PositionTagged<string> next = i == values.Length - 1 ?
                        suffix : // End of the list, grab the suffix
                        values[i + 1].Prefix; // Still in the list, grab the next prefix

                    bool? boolVal = null;
                    if (val.Value is bool)
                    {
                        boolVal = (bool)val.Value;
                    }

                    if (val.Value != null && (boolVal == null || boolVal.Value))
                    {
                        string valStr = val.Value as string;
                        if (valStr == null)
                        {
                            valStr = val.Value.ToString();
                        }
                        if (boolVal != null)
                        {
                            Debug.Assert(boolVal.Value);
                            valStr = name;
                        }

                        if (first)
                        {
                            WritePositionTaggedLiteral(writer, prefix);
                            first = false;
                        }
                        else
                        {
                            WritePositionTaggedLiteral(writer, attrVal.Prefix);
                        }

                        // Calculate length of the source span by the position of the next value (or suffix)
                        int sourceLength = next.Position - attrVal.Value.Position;

                        if (attrVal.Literal)
                        {
                            WriteLiteralTo(writer, valStr);
                        }
                        else
                        {
                            WriteTo(writer, valStr); // Write value
                        }
                        wroteSomething = true;
                    }
                }
                if (wroteSomething)
                {
                    WritePositionTaggedLiteral(writer, suffix);
                }
            }
        }

        private void WritePositionTaggedLiteral(TextWriter writer, string value, int position)
        {
            WriteLiteralTo(writer, value);
        }

        private void WritePositionTaggedLiteral(TextWriter writer, PositionTagged<string> value)
        {
            WritePositionTaggedLiteral(writer, value.Value, value.Position);
        }
    }
}
