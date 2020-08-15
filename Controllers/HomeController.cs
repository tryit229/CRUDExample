using CRUDExample.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CRUDExample.Controllers
{
    public class HomeController : Controller
    {

        private NorthwindRepository _northwindRepository;

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public async System.Threading.Tasks.Task<ActionResult> Products()
        {
            _northwindRepository = new NorthwindRepository();
            var list = await _northwindRepository.GetProductList();
            return View(list);
        }
    }
}