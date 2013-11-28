using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using Moq;
using NUnit.Framework;

namespace NPloy.Web.UnitTests
{
    [TestFixture]
    public class RouteTests
    {
        [TestCase("http://localhost/api/packages/search?searchTerm=*&targetFramework=4.0&includePrerelease=false")]
        [TestCase("http://localhost/api/packages/search")]
        public void ShouldResolveRoutes(string uri)
        {
            // setups
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var config = new HttpConfiguration();

            // act
            WebApiConfig.Register(config);
            var route = WebApi.RouteRequest(config, request);

            // asserts
            Assert.That(route.Controller.Name, Is.EqualTo("PackagesController"));
            Assert.That(route.Action, Is.EqualTo("Search"));
        }

        [TestCase("http://localhost/odata/packages"), Ignore("Web is WIP")]
        public void ShouldResolveOdataRoutes(string uri)
        {
            // setups
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var config = new HttpConfiguration();

            // act
            WebApiConfig.Register(config);
            var route = WebApi.RouteRequest(config, request);

            // asserts
            Assert.That(route.Controller.Name, Is.EqualTo("PackagesController"));
            Assert.That(route.Action, Is.EqualTo("Get"));
        }
    }

    public static class WebApi
    {
        public static RouteInfo RouteRequest(HttpConfiguration config, HttpRequestMessage request)
        {
            var httpRouteDataMock = (new Mock<IHttpRouteData>(MockBehavior.Loose));
            httpRouteDataMock.DefaultValue = DefaultValue.Mock;
            // create context
            var controllerContext = new HttpControllerContext(config, httpRouteDataMock.Object, request);

            // get route data
            var routeData = config.Routes.GetRouteData(request);
            RemoveOptionalRoutingParameters(routeData.Values);

            request.Properties[HttpPropertyKeys.HttpRouteDataKey] = routeData;
            controllerContext.RouteData = routeData;

            // get controller type
            var controllerDescriptor = new DefaultHttpControllerSelector(config).SelectController(request);
            controllerContext.ControllerDescriptor = controllerDescriptor;

            // get action name
            HttpActionDescriptor actionMapping;
            try
            {
                actionMapping = new ApiControllerActionSelector().SelectAction(controllerContext);
            }
            catch (HttpResponseException e)
            {

                Console.WriteLine(e.Response);

                throw;
            }


            return new RouteInfo
                {
                    Controller = controllerDescriptor.ControllerType,
                    Action = actionMapping.ActionName
                };
        }

        private static void RemoveOptionalRoutingParameters(IDictionary<string, object> routeValues)
        {
            var optionalParams = routeValues
                .Where(x => x.Value == RouteParameter.Optional)
                .Select(x => x.Key)
                .ToList();

            foreach (var key in optionalParams)
            {
                routeValues.Remove(key);
            }
        }
    }

    public class RouteInfo
    {
        public Type Controller { get; set; }

        public string Action { get; set; }
    }

}