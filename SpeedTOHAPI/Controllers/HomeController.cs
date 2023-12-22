using SpeedTOHAPI.Codes;
using SpeedTOHAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SpeedTOHAPI.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "SpeedPOS API";
            if (SessionHelper.GetSession() != null)
                return RedirectToAction("Index", "Help");
            else
                return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(LoginModel model)
        {
            try
            {
                string username = model.UserName.Replace("'", "");
                string password = model.Password.Replace("'", "");
                if (username != "" && password != "")
                {
                    if (username == System.Configuration.ConfigurationManager.AppSettings["APILoginUsername"].ToString() && password == System.Configuration.ConfigurationManager.AppSettings["APILoginPassword"].ToString())
                    {
                        SessionHelper.SetSession(new UserSession() { UserId = 0, UserName = System.Configuration.ConfigurationManager.AppSettings["APILoginUsername"].ToString() });
                        return RedirectToAction("Index", "Help");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Username or password is incorrect");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Please input username and password");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return View(model);
        }
    }
}
