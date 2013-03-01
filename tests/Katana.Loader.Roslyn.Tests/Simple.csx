
#r "Owin.Extensions.dll"
#r "Owin.Types.dll"
using Owin;

App.UseHandler((req, res) => res.StatusCode = 24601);

