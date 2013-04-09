using System.Web.Hosting;

namespace Microsoft.Owin.Security.Infrastructure
{
    internal static class HostingEnvironmentApi
    {
        private static readonly IApi Call = new Api();

        public static bool IsHosted
        {
            get { return Call.IsHosted; }
        }

        internal interface IApi
        {
            bool IsHosted { get; }
        }

        internal class Api : IApi
        {
            public bool IsHosted
            {
                get { return HostingEnvironment.IsHosted; }
            }
        }
    }
}
