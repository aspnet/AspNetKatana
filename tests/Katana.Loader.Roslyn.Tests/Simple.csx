
#r "Microsoft.Owin.dll"
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

App.Run(context =>
{
 context.Response.StatusCode = 24601;
 return Task.FromResult<object>(null);
});

