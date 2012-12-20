using System;

namespace Microsoft.Owin.Hosting.Starter
{
    public class HostingStarterAttribute : Attribute
    {
        public HostingStarterAttribute(Type hostingStarterType)
        {
            this.HostingStarterType = hostingStarterType;
        }
        public Type HostingStarterType { get; set; }
    }
}
