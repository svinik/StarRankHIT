using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StarRankHIT.Controllers
{
    public class ConsentController : Controller
    {
        // GET: Consent
        public ActionResult Index()
        {
            return View("Consent");
        }
    }
}