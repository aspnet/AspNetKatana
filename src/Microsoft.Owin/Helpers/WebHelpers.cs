using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Owin.Types.Helpers;

namespace Microsoft.Owin.Helpers
{
    public static class WebHelpers
    {
        public static NameValueCollection ParseNameValueCollection(string text)
        {
            var form = new NameValueCollection();

            OwinHelpers.ParseDelimited(
                        text,
                        new[] { '&' },
                        (a, b, c) => ((NameValueCollection)c).Add(a, b),
                        form);

            return form;
        }
    }
}
