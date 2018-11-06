using HearstWebService.App_Start;
using HearstWebService.Common;
using HearstWebService.Interfaces;
using System.Web.Http;
using System.Web.Optimization;
using System.Web.Routing;

namespace HearstWebService
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            SimpleInjectorWebApiInitializer.Initialize();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_Error()
        {
            var exception = Server.GetLastError();
            (SimpleInjectorWebApiInitializer.Container?.GetInstance<ILogger>() ?? new Logger()).Error("An unexpected error occured", exception);
        }
    }
}
