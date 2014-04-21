// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Owin.Infrastructure
{
    public interface ICookieManager
    {
        string GetRequestCookie(IOwinContext context, string key);
        void AppendResponseCookie(IOwinContext context, string key, string value, CookieOptions options);
        void DeleteCookie(IOwinContext context, string key, CookieOptions options);
    }
}
