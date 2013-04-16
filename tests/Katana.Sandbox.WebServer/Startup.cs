using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.Forms;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json;
using Owin;
using Owin.Types;
using Owin.Types.Extensions;

namespace Katana.Sandbox.WebServer
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseHandlerAsync(async (req, res, next) =>
            {
                req.TraceOutput.WriteLine("{0} {1}{2}", req.Method, req.PathBase, req.Path);
                await next();
                req.TraceOutput.WriteLine("{0} {1}{2}", res.StatusCode, req.PathBase, req.Path);
            });

            var dataProtectionProvider = new MachineKeyDataProtectionProvider();

            app.UseFormsAuthentication(new FormsAuthenticationOptions
            {
                LoginPath = "/Login",
                LogoutPath = "/Logout",
                DataProtection = dataProtectionProvider.Create("Katana.Sandbox.WebServer", "Forms Cookie"),
                Provider = new FormsAuthenticationProvider()
            });

            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions
            {
                DataProtection = dataProtectionProvider.Create("Katana.Sandbox.WebServer", "OAuth Bearer Token"),
                Provider = new OAuthBearerAuthenticationProvider
                {
                    OnValidateIdentity = async context =>
                    {

                    }
                }
            });

            app.UseOAuthAuthorizationServer(new OAuthAuthorizationServerOptions
            {
                AuthorizeEndpointPath = "/Authorize",
                TokenEndpointPath = "/Token",
                DataProtection = dataProtectionProvider.Create("Katana.Sandbox.WebServer", "OAuth Bearer Token"),
                Provider = new OAuthAuthorizationServerProvider
                {
                    OnLookupClientId = async context =>
                    {
                        if (context.ClientId == "123456")
                        {
                            context.ClientFound("abcdef", "http://localhost:18429/ClientApp.aspx");
                        }
                    },
                    OnAuthorizeEndpoint = async context =>
                    {
                        var request = new OwinRequest(context.Environment);
                        var response = new OwinResponse(context.Environment);

                        var user = await request.Authenticate("Forms", "Basic");
                        if (user == null)
                        {
                            response.Unauthorized("Forms", "Basic");
                            context.RequestCompleted = true;
                        }
                        else
                        {
                            request.User = user;
                        }
                    },
                    OnTokenEndpoint = async context =>
                    {
                        context.Issue();
                    }
                }
            });

            var config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "api/{controller}");
            app.UseWebApi(config);
        }
    }

    static class MoreExtensions
    {
        public static async Task<ClaimsPrincipal> Authenticate(this OwinRequest request, params string[] authenticationTypes)
        {
            var identities = new List<ClaimsIdentity>();
            await request.Authenticate(authenticationTypes, identity => identities.Add(new ClaimsIdentity(identity)));
            return identities.Count != 0 ? new ClaimsPrincipal(identities) : null;
        }
    }
}