using System;
using Microsoft.Owin.Builder;
using Microsoft.Owin.Infrastructure;
using Owin;
using Shouldly;
using Xunit;

namespace Microsoft.Owin.Tests.Builder
{
    public class AppBuilderCookieTests
    {
        private const string CookieManagerProperty = "infrastructure.CookieManager";
        private const string ChunkingCookieManagerProperty = "infrastructure.ChunkingCookieManager";

        [Fact]
        public void SetDefaultCookieManager_NullBuilderShouldThrow()
        {
            var app = (IAppBuilder) null;

            var exception = Should.Throw<ArgumentNullException>(() => app.SetDefaultCookieManager(null));
            exception.ParamName.ShouldBe("app");
        }

        [Fact]
        public void SetDefaultCookieManager_NullManagerShouldRemoveProperty()
        {
            var app = new AppBuilder
            {
                Properties =
                {
                    [CookieManagerProperty] = new CookieManager()
                }
            };

            app.SetDefaultCookieManager(null);

            app.Properties.ShouldNotContainKey(CookieManagerProperty);
        }

        [Fact]
        public void SetDefaultCookieManager_NonNullManagerShouldOverrideProperty()
        {
            var manager = new CookieManager();

            var app = new AppBuilder
            {
                Properties =
                {
                    [CookieManagerProperty] = new CookieManager()
                }
            };

            app.SetDefaultCookieManager(manager);

            app.Properties[CookieManagerProperty].ShouldBeSameAs(manager);
        }

        [Fact]
        public void SetDefaultChunkingCookieManager_NullBuilderShouldThrow()
        {
            var app = (IAppBuilder) null;

            var exception = Should.Throw<ArgumentNullException>(() => app.SetDefaultChunkingCookieManager(null));
            exception.ParamName.ShouldBe("app");
        }

        [Fact]
        public void SetDefaultChunkingCookieManager_NullManagerShouldRemoveProperty()
        {
            var app = new AppBuilder
            {
                Properties =
                {
                    [ChunkingCookieManagerProperty] = new ChunkingCookieManager()
                }
            };

            app.SetDefaultChunkingCookieManager(null);

            app.Properties.ShouldNotContainKey(ChunkingCookieManagerProperty);
        }

        [Fact]
        public void SetDefaultChunkingCookieManager_NonNullManagerShouldOverrideProperty()
        {
            var manager = new ChunkingCookieManager();

            var app = new AppBuilder
            {
                Properties =
                {
                    [ChunkingCookieManagerProperty] = new ChunkingCookieManager()
                }
            };

            app.SetDefaultChunkingCookieManager(manager);

            app.Properties[ChunkingCookieManagerProperty].ShouldBeSameAs(manager);
        }

        [Fact]
        public void GetDefaultCookieManager_NullBuilderShouldThrow()
        {
            var app = (IAppBuilder) null;

            var exception = Should.Throw<ArgumentNullException>(() => app.GetDefaultCookieManager());
            exception.ParamName.ShouldBe("app");
        }

        [Fact]
        public void GetDefaultCookieManager_MissingManagerShouldReturnDefaultInstance()
        {
            var app = new AppBuilder();

            app.GetDefaultCookieManager().ShouldBeTypeOf(typeof(CookieManager));
        }

        [Fact]
        public void GetDefaultCookieManager_InvalidManagerShouldReturnDefaultInstance()
        {
            var app = new AppBuilder
            {
                Properties =
                {
                    [CookieManagerProperty] = new object()
                }
            };

            app.GetDefaultCookieManager().ShouldBeTypeOf(typeof(CookieManager));
        }

        [Fact]
        public void GetDefaultCookieManager_ValidManagerShouldBeReturned()
        {
            var manager = new CookieManager();

            var app = new AppBuilder
            {
                Properties =
                {
                    [CookieManagerProperty] = manager
                }
            };

            app.GetDefaultCookieManager().ShouldBeSameAs(manager);
        }

        [Fact]
        public void GetDefaultChunkingCookieManager_NullBuilderShouldThrow()
        {
            var app = (IAppBuilder) null;

            var exception = Should.Throw<ArgumentNullException>(() => app.GetDefaultChunkingCookieManager());
            exception.ParamName.ShouldBe("app");
        }

        [Fact]
        public void GetDefaultChunkingCookieManager_MissingManagerShouldReturnDefaultInstance()
        {
            var app = new AppBuilder();

            app.GetDefaultChunkingCookieManager().ShouldBeTypeOf(typeof(ChunkingCookieManager));
        }

        [Fact]
        public void GetDefaultChunkingCookieManager_InvalidManagerShouldReturnDefaultInstance()
        {
            var app = new AppBuilder
            {
                Properties =
                {
                    [ChunkingCookieManagerProperty] = new object()
                }
            };

            app.GetDefaultChunkingCookieManager().ShouldBeTypeOf(typeof(ChunkingCookieManager));
        }

        [Fact]
        public void GetDefaultChunkingCookieManager_ValidManagerShouldBeReturned()
        {
            var manager = new ChunkingCookieManager();

            var app = new AppBuilder
            {
                Properties =
                {
                    [ChunkingCookieManagerProperty] = manager
                }
            };

            app.GetDefaultChunkingCookieManager().ShouldBeSameAs(manager);
        }
    }
}
