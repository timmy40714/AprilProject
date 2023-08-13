using Microsoft.AspNetCore.Mvc;
using prjCatChaOnlineShop.Models;

namespace prjCatChaOnlineShop.Controllers.Home
{
    public class CartController : Controller
    {
        private readonly 貓抓抓Context _context;
        public CartController(貓抓抓Context conetxt)
        {
            _context = conetxt;
        }
        public IActionResult product()
        {
            var products = _context.ShopProductTotal.Where(p => p.ProductId == 1);
            return View(products);
        }

        public IActionResult ConfrimOrder()
        { 
            return View();
        }
        public IActionResult Checkout() 
        {
            return View();
        }
        public IActionResult Cart()
        {
            return View();
        }
    }
}
