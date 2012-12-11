using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Owin.Host.HttpListener.Tests
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class OwinServerFactoryAttributeTests
    {
        private readonly AppFunc _notImplemented = env => { throw new NotImplementedException(); };

        [Fact]
        public void InitializeNullProperties_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => OwinServerFactoryAttribute.Initialize(null));
        }

        [Fact]
        public void Initialize_PopulatesExpectedFields()
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            OwinServerFactoryAttribute.Initialize(properties);

            Assert.Equal("1.0", properties["owin.Version"]);
            Assert.IsType(typeof(OwinHttpListener), properties["Microsoft.Owin.Host.HttpListener.OwinHttpListener"]);
            Assert.IsType(typeof(System.Net.HttpListener), properties["System.Net.HttpListener"]);
        }

        [Fact]
        public void CreateNullAppFunc_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => OwinServerFactoryAttribute.Create(null, new Dictionary<string, object>()));
        }

        [Fact]
        public void CreateNullProperties_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => OwinServerFactoryAttribute.Create(_notImplemented, null));
        }

        [Fact]
        public void CreateEmptyProperties_Success()
        {
            OwinServerFactoryAttribute.Create(_notImplemented, new Dictionary<string, object>());
        }
    }
}
