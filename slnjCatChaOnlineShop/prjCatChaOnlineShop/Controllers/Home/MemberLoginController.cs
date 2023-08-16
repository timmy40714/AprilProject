using Microsoft.AspNetCore.Mvc;
using prjCatChaOnlineShop.IvanModels;
using prjCatChaOnlineShop.Services.Models;
using prjCatChaOnlineShop.Services.Models;

namespace prjCatChaOnlineShop.Controllers.Home
{
    public class MemberLoginController : Controller
    {
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        //登入頁面
        public IActionResult Login(CLoginModel vm)
        {
            ShopMemberInfo customer = (new CCustomerFactory()).queryByEmail(vm.txtEmail);
            if (customer != null && customer.Password.Equals(vm.txtPassword)) 
            {
                
                return RedirectToAction("Index", "Index");
            }

            return View();
        }

        public IActionResult ForgetPassword()
        {

            return View();
        }

        public IActionResult RegisterMember() 
        {
            return View();
        }
    }
}
