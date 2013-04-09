using System.Security.Principal;

namespace Microsoft.Owin.Security.Forms
{
    public class FormsValidateIdentityContext
    {
        public FormsValidateIdentityContext(IIdentity identity)
        {
            Identity = identity;
        }

        public IIdentity Identity { get; private set; }

        public void ReplaceIdentity(IIdentity identity)
        {
            Identity = identity;
        }

        public void RejectIdentity()
        {
            Identity = null;
        }
    }
}