// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Microsoft.Owin.Security.Cookies
{
    internal static class DefaultBehavior
    {
        internal static readonly Action<CookieApplyRedirectContext> ApplyRedirect = context =>
        {
            if (IsAjaxRequest(context.Request))
            {
                var respondedJson = new RespondedJson
                {
                    Status = context.Response.StatusCode,
                    Headers = new RespondedJson.RespondedJsonHeaders
                    {
                        Location = context.RedirectUri
                    },
                };

                context.Response.StatusCode = 200;
                context.Response.Headers.Append("X-Responded-JSON", respondedJson.ToString());
            }
            else
            {
                context.Response.Redirect(context.RedirectUri);
            }
        };

        private static bool IsAjaxRequest(IOwinRequest request)
        {
            IReadableStringCollection query = request.Query;
            if (query != null)
            {
                if (query["X-Requested-With"] == "XMLHttpRequest")
                {
                    return true;
                }
            }

            IHeaderDictionary headers = request.Headers;
            if (headers != null)
            {
                if (headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return true;
                }
            }
            return false;
        }

        [DataContract]
        private class RespondedJson
        {
            public static readonly DataContractJsonSerializer Serializer = new DataContractJsonSerializer(typeof(RespondedJson));

            [DataMember(Name = "status", Order = 1)]
            public int Status { get; set; }

            [DataMember(Name = "headers", Order = 2)]
            public RespondedJsonHeaders Headers { get; set; }

            [DataContract]
            public class RespondedJsonHeaders
            {
                [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Used by serialization")]
                [DataMember(Name = "location", Order = 1)]
                public string Location { get; set; }
            }

            public override string ToString()
            {
                using (var memory = new MemoryStream())
                {
                    Serializer.WriteObject(memory, this);
                    string responded = Encoding.ASCII.GetString(memory.ToArray());
                    return responded;
                }
            }
        }
    }
}
