using Orchard.Mvc.Routes;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;

namespace Lombiq.ArchivedLinks.Routes
{
    public class ArchivedLinkRouteProvider : IRouteProvider
    {
        public void GetRoutes(ICollection<RouteDescriptor> routes)
        {
            foreach (var routeDescriptor in GetRoutes())
            {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes()
        {
            const string Area = "Lombiq.ArchivedLinks";
            const string Controller = "ArchivedLink";

            return new[]
            {
                new RouteDescriptor
                {
                    Priority = 5,
                    Route = new Route(
                        "jumpifworking/{originalUrl}",
                        new RouteValueDictionary
                        {
                            { "area", Area },
                            { "controller", Controller },
                            { "action", "Index" },
                            { "originalUrl", UrlParameter.Optional}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary
                        {
                            { "area", Area }
                        },
                        new MvcRouteHandler())
                },
            };
        }
    }
}