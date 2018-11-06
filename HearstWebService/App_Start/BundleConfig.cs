using System.Web.Optimization;

namespace HearstWebService.App_Start
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/jquery/js").Include(
                        "~/Scripts/jquery-{version}.js"));
            bundles.Add(new ScriptBundle("~/jqueryval/js").Include(
                        "~/Scripts/jquery.validate*"));
            bundles.Add(new ScriptBundle("~/bootstrap/js").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/app/js/status").Include(
                        "~/Scripts/app/status.js"));


            bundles.Add(new StyleBundle("~/bootstrap/css").Include(
                      "~/Content/bootstrap.css"));
            bundles.Add(new StyleBundle("~/shared/css").Include(
                      "~/Content/css/site.css"));
            bundles.Add(new StyleBundle("~/loader/css").Include(
                      "~/Content/css/loader.css"));
        }
    }
}