using System.Security.Claims;

namespace Microsoft.Owin.Security.Infrastructure
{
    public struct SecurityHelperLookupResult
    {
        private readonly bool _shouldHappen;
        private readonly ClaimsIdentity _identity;

        public SecurityHelperLookupResult(bool shouldHappen)
        {
            _shouldHappen = shouldHappen;
            _identity = null;
        }

        public SecurityHelperLookupResult(bool shouldHappen, ClaimsIdentity identity)
        {
            _shouldHappen = shouldHappen;
            _identity = identity;
        }

        public bool ShouldHappen
        {
            get { return _shouldHappen; }
        }

        public ClaimsIdentity Identity
        {
            get { return _identity; }
        }
    }
}