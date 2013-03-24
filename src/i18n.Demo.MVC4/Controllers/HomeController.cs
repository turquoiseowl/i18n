using System.Web.Mvc;

namespace i18n.Demo.MVC4.Controllers
{
    public class HomeController : i18n.I18NController
    {
        public ActionResult Index()
        {
            ViewBag.Message = __("Modify this template to jump-start your ASP.NET MVC application.");

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = __("Your app description page.");

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = __("Your contact page.");

            return View();
        }

        public ViewResult TestLanguage()
        {
            return View();
        }

        public EmptyResult SetLanguageInSession(string language)
        {
            I18NSession session = new I18NSession();
            session.Set(this.HttpContext, language);
            return new EmptyResult();
        }
    }
}
