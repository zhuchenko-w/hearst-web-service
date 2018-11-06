[assembly: WebActivator.PostApplicationStartMethod(typeof(HearstWebService.App_Start.SimpleInjectorWebApiInitializer), "Initialize")]

namespace HearstWebService.App_Start
{
    using System.Web.Http;
    using SimpleInjector;
    using SimpleInjector.Integration.WebApi;
    using System.Web.Mvc;
    using SimpleInjector.Integration.Web.Mvc;
    using System.Reflection;
    using SimpleInjector.Integration.Web;
    using HearstWebService.Interfaces;
    using HearstWebService.Common;
    using HearstWebService.Data.Accessor;
    using HearstWebService.BusinessLogic.StoredProcedures;
    using HearstWebService.Common.Helpers;
    using System;
    using HearstWebService.Common.Cache;
    using HearstWebService.BusinessLogic.Reports;

    public static class SimpleInjectorWebApiInitializer
    {
        public static Container Container;

        public static void Initialize()
        {
            Container = new Container();
            Container.Options.DefaultScopedLifestyle = new WebRequestLifestyle();

            InitializeContainer(Container);

            Container.RegisterWebApiControllers(GlobalConfiguration.Configuration);
            Container.RegisterMvcControllers(Assembly.GetExecutingAssembly());

            Container.Verify();

            GlobalConfiguration.Configuration.DependencyResolver = 
                new SimpleInjectorWebApiDependencyResolver(Container);
            DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(Container));
        }
     
        private static void InitializeContainer(Container container)
        {
            container.Register<ILogger, Logger>(Lifestyle.Singleton);
            container.Register<ICache, InMemoryCache>(Lifestyle.Singleton);
            container.Register<IStoredProceduresLogic, StoredProceduresLogic>(Lifestyle.Singleton);
            container.Register<IReportLogic, ReportLogic>(Lifestyle.Singleton);

            container.Register(() => new Lazy<ILogger>(container.GetInstance<ILogger>), Lifestyle.Singleton);
            container.Register(() => new Lazy<ICache>(container.GetInstance<ICache>), Lifestyle.Singleton);
            container.Register(() => new Lazy<IStoredProceduresLogic>(container.GetInstance<IStoredProceduresLogic>), Lifestyle.Singleton);
            container.Register(() => new Lazy<IReportLogic>(container.GetInstance<IReportLogic>), Lifestyle.Singleton);

            container.Register<IDbAccessor>(() =>
                new DbAccessor(
                    ConfigHelper.Instance.ConnectionString,
                    ConfigHelper.Instance.ReportValidParamValuesCacheExpirationHours,
                    ConfigHelper.Instance.SettingValuesCacheExpirationHours,
                    container.GetInstance<Lazy<ICache>>(),
                    container.GetInstance<Lazy<ILogger>>()),
                Lifestyle.Singleton);

            container.Register(() => new Lazy<IDbAccessor>(container.GetInstance<IDbAccessor>), Lifestyle.Singleton);
        }
    }
}