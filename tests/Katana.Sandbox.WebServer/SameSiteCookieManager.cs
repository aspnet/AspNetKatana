using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin;
using Microsoft.Owin.Infrastructure;

namespace Katana.Sandbox.WebServer
{
    public class SameSiteCookieManager : ICookieManager
    {
        private readonly ICookieManager _innerManager;

        public SameSiteCookieManager()
            : this(new CookieManager())
        {   
        }

        public SameSiteCookieManager(ICookieManager innerManager)
        {
            _innerManager = innerManager;
        }

        public void AppendResponseCookie(IOwinContext context, string key, string value, CookieOptions options)
        {
            CheckSameSite(context, options);
            _innerManager.AppendResponseCookie(context, key, value, options);
        }

        public void DeleteCookie(IOwinContext context, string key, CookieOptions options)
        {
            CheckSameSite(context, options);
            _innerManager.DeleteCookie(context, key, options);
        }

        public string GetRequestCookie(IOwinContext context, string key)
        {
            return _innerManager.GetRequestCookie(context, key);
        }

        private void CheckSameSite(IOwinContext context, CookieOptions options)
        {
            if (IsIOS12(context) && options.SameSite == SameSiteMode.None)
            {
                // IOS12 treats SameSite=None as SameSite=Strict. Exclude the option instead.
                // https://bugs.webkit.org/show_bug.cgi?id=198181
                options.SameSite = null;
            }
        }

        // https://myip.ms/view/comp_browsers/8568/Safari_12.html
        public static bool IsIOS12(IOwinContext context)
        {
            // TODO: Use your User Agent library of choice here.
            var userAgent = context.Request.Headers["User-Agent"];
            return userAgent.Contains("iPad; CPU OS 12")
                || userAgent.Contains("CPU iPhone OS 12"); // Also covers iPod touch
        }
    }
}