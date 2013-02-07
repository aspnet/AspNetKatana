using Owin.Types.Helpers;
using System;

namespace Owin.Types.Extensions
{
#region OwinRequestExtensions.Forwarded.

    internal static partial class OwinRequestExtensions
    {
        public static string GetForwardedScheme(this OwinRequest request)
        {
            return OwinHelpers.GetForwardedScheme(request);
        }

        public static string GetForwardedHost(this OwinRequest request)
        {
            return OwinHelpers.GetForwardedHost(request);
        }

        public static Uri GetForwardedUri(this OwinRequest request)
        {
            return OwinHelpers.GetForwardedUri(request);
        }
        
        public static OwinRequest ApplyForwardedScheme(this OwinRequest request)
        {
            return OwinHelpers.ApplyForwardedScheme(request);
        }
        
        public static OwinRequest ApplyForwardedHost(this OwinRequest request)
        {
            return OwinHelpers.ApplyForwardedHost(request);
        }
        
        public static OwinRequest ApplyForwardedUri(this OwinRequest request)
        {
            return OwinHelpers.ApplyForwardedUri(request);
        }
    }
#endregion

#region OwinRequestExtensions.MethodOverride

    internal static partial class OwinRequestExtensions
    {
        public static string GetMethodOverride(this OwinRequest request)
        {
            return OwinHelpers.GetMethodOverride(request);
        }

        public static OwinRequest ApplyMethodOverride(this OwinRequest request)
        {
            return OwinHelpers.ApplyMethodOverride(request);
        }
    }
#endregion

}
