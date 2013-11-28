using System.Web.Http;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Routing.Conventions;
using NuGet.Server.DataServices;

namespace NPloy.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {


            config.EnableQuerySupport();
            //var conventions = ODataRoutingConventions.CreateDefault();
            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<Package>("packages");
            var model = modelBuilder.GetEdmModel();
            config.Routes.MapODataRoute("ODataRoute", "odata", model);

            config.Routes.MapHttpRoute(
                name: "ActionApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new {id = RouteParameter.Optional}
                );


            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional });
        }
    }
}
