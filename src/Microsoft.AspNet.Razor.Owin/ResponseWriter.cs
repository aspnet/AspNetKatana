// -----------------------------------------------------------------------
// <copyright file="ResponseWriter.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gate;

namespace Microsoft.AspNet.Razor.Owin
{
    public class ResponseWriter : TextWriter
    {
        public ResponseWriter(Response response)
        {
            Response = response;
        }

        public Response Response { get; private set; }
        
        public override Encoding Encoding
        {
            get { return Response.Encoding; }
        }

        public override void Write(char c)
        {
            Response.Write(c.ToString());
        }
    }
}
