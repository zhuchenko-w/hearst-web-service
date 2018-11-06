using System.Linq;
using HearstWebService.App_Start;
using HearstWebService.Interfaces;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using HearstWebService.Common.Helpers;
using HearstWebService.Data.Model;
using System.Web;

namespace HearstWebService.Attributes
{
    public class ApiAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var userCookie = actionContext.Request.Headers.GetCookies(ConfigHelper.Instance.UserCookieName)
                .FirstOrDefault()?[ConfigHelper.Instance.UserCookieName].Value;

            if (!string.IsNullOrEmpty(userCookie))
            {
                var user = User.DeserializeUser(userCookie);
                return SimpleInjectorWebApiInitializer.Container
                    .GetInstance<IAuthenticationLogic>()
                    .IsUserAuthenticated(user);
            }

            return false;
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Redirect);
            var relativeUri = $"/Login?returnUrl={HttpUtility.UrlEncode(actionContext.Request.RequestUri.AbsoluteUri)}";
            var baseUri = new System.Uri(actionContext.Request.RequestUri.AbsoluteUri.Replace(actionContext.Request.RequestUri.AbsolutePath, ""));
            actionContext.Response.Headers.Location = new System.Uri(baseUri, relativeUri);
        }
    }
}