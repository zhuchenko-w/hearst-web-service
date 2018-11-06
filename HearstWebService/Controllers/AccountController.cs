using HearstWebService.Attributes;
using HearstWebService.Common.Helpers;
using HearstWebService.Data.Model;
using HearstWebService.Interfaces;
using HearstWebService.Models;
using System;
using System.Web;
using System.Web.Mvc;

namespace HearstWebService.Controllers
{
    public class AccountController : Controller
    {
        private const string DefaultRedirectRoute = "\\";

        private readonly Lazy<IAuthenticationLogic> _authenticationLogic;

        public AccountController(Lazy<IAuthenticationLogic> authenticationLogic)
        {
            _authenticationLogic = authenticationLogic;
        }

        [AllowAnonymous]
        [AddDomainUser]
        public ActionResult Login(string returnUrl, User domainUser)
        {
            if (domainUser != null && _authenticationLogic.Value.IsUserAuthenticated(domainUser))
            {
                return Redirect(string.IsNullOrEmpty(returnUrl) ? DefaultRedirectRoute : returnUrl);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = _authenticationLogic.Value.Login(model.Domain, model.Username, model.Password);
                if (user == null)
                {
                    ModelState.AddModelError("", "Неверные учетные данные");
                }
                else
                {
                    var userCookie = new HttpCookie(ConfigHelper.Instance.UserCookieName, user.SerializedUser);
                    userCookie.Expires = DateTime.Now.AddDays(365);
                    HttpContext.Response.Cookies.Add(userCookie);

                    return Redirect(string.IsNullOrEmpty(returnUrl) ? DefaultRedirectRoute : returnUrl);
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [AddDomainUser]
        public ActionResult LogOut(User domainUser)
        {
            if (domainUser != null)
            {
                _authenticationLogic.Value.Logout(domainUser.Domain, domainUser.Username);
                var userCookie = new HttpCookie(ConfigHelper.Instance.UserCookieName, domainUser.SerializedUser);
                userCookie.Expires = DateTime.Now.AddDays(-1);
                HttpContext.Response.Cookies.Add(userCookie);
            }

            return RedirectToAction("Login");
        }
    }
}