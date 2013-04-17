using System.Collections.Generic;

namespace Microsoft.Owin.Security.Provider
{
    public abstract class BaseContext
    {
        public BaseContext(IDictionary<string, object> environment)
        {
            Environment = environment;
        }

        public IDictionary<string, object> Environment { get; private set; }        
    }

    public abstract class EndpointContext : BaseContext
    {
        public EndpointContext(IDictionary<string, object> environment) : base(environment)
        {
        }

        public bool IsRequestCompleted { get; private set; }

        public void RequestCompleted()
        {
            IsRequestCompleted = true;
        }
    }
}
