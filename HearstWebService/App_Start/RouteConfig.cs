using System.Web.Mvc;
using System.Web.Routing;

namespace HearstWebService
{
	public class RouteConfig
	{
		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
			routes.IgnoreRoute("favicon.ico");
			routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*[/\\])?favicon\.((ico)|(png))(/.*)?" });

            routes.MapRoute(
				name: "ErrorDefault",
				url: "Error",
				defaults:
					new
					{
						controller = "Error",
						action = "Index"
					}
			);

			routes.MapRoute(
                name: "ErrorNotFound",
				url: "NotFoundPage",
				defaults:
					new
					{
						controller = "Error",
						action = "NotFoundPage"
					}
			);

            routes.MapRoute(
                name: "ErrorForbidden",
                url: "Forbidden",
                defaults:
                    new
                    {
                        controller = "Error",
                        action = "Forbidden"
                    }
            );

            routes.MapRoute(
                name: "ErrorInvalidParameter",
                url: "InvalidParameter",
                defaults:
                    new
                    {
                        controller = "Error",
                        action = "InvalidParameter"
                    }
            );

            routes.MapRoute(
                name: "Default", 
				url: "",
                defaults: 
                    new
                    {
                        controller = "Home",
                        action = "Index",
                    }
            );

            routes.MapMvcAttributeRoutes();
        }
	}
}