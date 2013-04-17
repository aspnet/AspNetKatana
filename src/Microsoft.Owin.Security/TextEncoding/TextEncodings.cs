using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Owin.Security.TextEncoding
{
    public static class TextEncodings
    {
        private static readonly ITextEncoding Base64Instance = new Base64TextEncoding();
        private static readonly ITextEncoding Base64UrlInstance = new Base64UrlTextEncoding();

        public static ITextEncoding Base64
        {
            get { return Base64Instance; }
        }

        public static ITextEncoding Base64Url
        {
            get { return Base64UrlInstance; }
        }
    }
}
