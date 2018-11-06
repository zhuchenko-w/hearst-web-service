using HearstWebService.Common.Helpers;
using HearstWebService.Data.Model;
using System.Web.Mvc;

namespace HearstWebService.Attributes
{
    public class AddDomainUserAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            var userCookie = filterContext.HttpContext.Request.Cookies.Get(ConfigHelper.Instance.UserCookieName)?.Value;
            if (!string.IsNullOrEmpty(userCookie))
            {
                filterContext.ActionParameters["domainUser"] = User.DeserializeUser(userCookie);
            }
        }
    }
}