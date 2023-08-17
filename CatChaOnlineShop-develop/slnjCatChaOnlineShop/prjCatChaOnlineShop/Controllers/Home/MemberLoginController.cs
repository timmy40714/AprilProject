using Microsoft.AspNetCore.Mvc;
using prjCatChaOnlineShop.Models;
using prjCatChaOnlineShop.Models.CDictionary;
using prjCatChaOnlineShop.Models.ViewModels;
using System.Text.Json;

namespace prjCatChaOnlineShop.Controllers.Home
{
    public class MemberLoginController : Controller
    {
        public IActionResult Login(CLoginModel vm)
        {
            ShopMemberInfo user = (new cachaContext()).ShopMemberInfo.FirstOrDefault(
                t => t.Email.Equals(vm.txtEmail) && t.Password.Equals(vm.txtPassword));
            if (user != null && user.Password.Equals(vm.txtPassword))
            {
                string Json = JsonSerializer.Serialize(user);
                HttpContext.Session.SetString(CDictionary.SK_LOINGED_USER, Json);
                return RedirectToAction("Indexn");
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
