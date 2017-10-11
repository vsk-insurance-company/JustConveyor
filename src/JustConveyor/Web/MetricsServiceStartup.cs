using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web.Cors;
using System.Web.Http;
using JustConveyor.Contracts.Dependencies;
using JustConveyor.Contracts.Settings;
using Microsoft.Owin.Cors;
using Owin;

namespace JustConveyor.Web
{
    internal class MetricsServiceStartup
    {
        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public void Configuration(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "justconveyor/api/{controller}/{id}",
                defaults: new {id = RouteParameter.Optional}
                );
            config.Services.Replace(typeof(IContentNegotiator),
                new JsonContentNegotiator(config.Formatters.JsonFormatter));

            var settings = Injection.InjectionProvider.Get<MetricsServiceSettings>("conveyor:metrics-service-settings");
            var corsPolicy = new CorsPolicy
            {
                AllowAnyHeader = true,
                AllowAnyOrigin = true
            };
            settings.CorsAddresses.ForEach(corsPolicy.Origins.Add);

            appBuilder.UseCors(new CorsOptions
            {
                PolicyProvider = new CorsPolicyProvider
                {
                    PolicyResolver = ctx =>
                    {
                        return Task.FromResult(corsPolicy);
                    }
                }
            });
            appBuilder.UseWebApi(config);
        }
    }
}