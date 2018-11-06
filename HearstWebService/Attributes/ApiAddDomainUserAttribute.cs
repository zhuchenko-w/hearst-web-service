using HearstWebService.Common.Helpers;
using HearstWebService.Data.Model;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace HearstWebService.Attributes
{
    public class ApiAddDomainUserAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            base.OnActionExecuting(actionContext);

            var userCookie = actionContext.Request.Headers.GetCookies(ConfigHelper.Instance.UserCookieName)
                .FirstOrDefault()?[ConfigHelper.Instance.UserCookieName].Value;
            if (!string.IsNullOrEmpty(userCookie))
            {
                actionContext.ActionArguments["domainUser"] = User.DeserializeUser(userCookie);
            }
        }
    }
}