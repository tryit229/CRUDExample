using CRUDExample.Models;
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

        public async System.Threading.Tasks.Task<ActionResult> Products()
        {
            _northwindRepository = new NorthwindRepository();
            var list = await _northwindRepository.GetProductList();
            return View(list);
        }

        [HttpPost]
        public async System.Threading.Tasks.Task<ActionResult> InsertProduct(ProductsModel Product)
        {
            _northwindRepository = new NorthwindRepository();
            var flag = await _northwindRepository.InsertProduct(Product);
            //可考慮回傳訊息:新增成功、失敗，原因。
            return RedirectToAction("Products", "Home");

        }

        [HttpPost]
        public async System.Threading.Tasks.Task<ActionResult> EditPrice(ProductsModel Product)
        {
            _northwindRepository = new NorthwindRepository();
            var flag = await _northwindRepository.UpdatePrice(Product.ProductID, Product.UnitPrice);
            //可考慮回傳訊息:新增成功、失敗，原因。
            return RedirectToAction("Products", "Home");
        }

        [HttpPost]
        public async System.Threading.Tasks.Task<ActionResult> OffShelf(int ProductID)
        {
            _northwindRepository = new NorthwindRepository();
            var flag = await _northwindRepository.OffShelf(ProductID);
            //可考慮回傳訊息:新增成功、失敗，原因。
            return RedirectToAction("Products", "Home");

        }
    }
}