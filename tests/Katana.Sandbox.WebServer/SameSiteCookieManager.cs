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

        public SameSiteCookieManager() : this(new CookieManager())
        {
        }

        public SameSiteCookieManager(ICookieManager innerManager)
        {
            _innerManager = innerManager;
        }

        public void AppendResponseCookie(IOwinContext context, string key, string value,
                                         CookieOptions options)
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
            if (options.SameSite == SameSiteMode.None && DisallowsSameSiteNone(context))
            {
                options.SameSite = null;
            }
        }

        public static bool DisallowsSameSiteNone(IOwinContext context)
        {
            // TODO: Use your User Agent library of choice here.
            var userAgent = context.Request.Headers["User-Agent"];
            return DisallowsSameSiteNone(userAgent);
        }

        public static bool DisallowsSameSiteNone(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
            {
                return false;
            }

            // Cover all iOS based browsers here. This includes:
            // - Safari on iOS 12 for iPhone, iPod Touch, iPad
            // - WkWebview on iOS 12 for iPhone, iPod Touch, iPad
            // - Chrome on iOS 12 for iPhone, iPod Touch, iPad
            // All of which are broken by SameSite=None, because they use the iOS networking stack
            if (userAgent.Contains("CPU iPhone OS 12") || userAgent.Contains("iPad; CPU OS 12"))
            {
                return true;
            }

            // Cover Mac OS X based browsers that use the Mac OS networking stack. This includes:
            // - Safari on Mac OS X.
            // This does not include:
            // - Chrome on Mac OS X
            // Because they do not use the Mac OS networking stack.
            if (userAgent.Contains("Macintosh; Intel Mac OS X 10_14") &&
                userAgent.Contains("Version/") && userAgent.Contains("Safari"))
            {
                return true;
            }

            // Cover Chrome 50-69, because some versions are broken by SameSite=None, 
            // and none in this range require it.
            // Note: this covers some pre-Chromium Edge versions, 
            // but pre-Chromium Edge does not require SameSite=None.
            if (userAgent.Contains("Chrome/5") || userAgent.Contains("Chrome/6"))
            {
                return true;
            }

            return false;
        }
    }
}