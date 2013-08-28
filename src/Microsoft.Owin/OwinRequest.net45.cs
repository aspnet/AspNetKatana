// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NET40

using System.IO;
using System.Threading.Tasks;
using Microsoft.Owin.Infrastructure;

namespace Microsoft.Owin
{
    /// <summary>
    /// This wraps OWIN environment dictionary and provides strongly typed accessors.
    /// </summary>
    public partial class OwinRequest : IOwinRequest
    {
        /// <summary>
        /// Asynchronously reads and parses the request body as a form.
        /// </summary>
        /// <returns>The parsed form data.</returns>
        public async Task<IFormCollection> ReadFormAsync()
        {
            var form = Get<IFormCollection>("Microsoft.Owin.Form#collection");
            if (form == null)
            {
                string text;
                using (var reader = new StreamReader(Body))
                {
                    text = await reader.ReadToEndAsync();
                }
                form = OwinHelpers.GetForm(text);
                Set("Microsoft.Owin.Form#collection", form);
            }

            return form;
        }
    }
}

#else

using ResharperCodeFormattingWorkaround = System.Object;

#endif
