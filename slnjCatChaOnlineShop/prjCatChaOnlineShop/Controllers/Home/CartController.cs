﻿using Microsoft.AspNetCore.Mvc;
using prjCatChaOnlineShop.Models;
using prjCatChaOnlineShop.Models.CDictionary;
using prjCatChaOnlineShop.Models.CModels;
using prjCatChaOnlineShop.Models.ViewModels;
using prjCatChaOnlineShop.Services.Function;
using System.Text.Json;

namespace prjCatChaOnlineShop.Controllers.Home
{
    public class CartController : Controller
    {
        private readonly CheckoutService _checkoutService;
        private readonly cachaContext _context;
        private readonly IWebHostEnvironment _host;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ProductService _productService;
        
        public CartController(cachaContext context, IWebHostEnvironment host, IHttpContextAccessor httpContextAccessor, ProductService productService,CheckoutService checkoutService)
        {
            _context = context;
            _host = host;
            _httpContextAccessor = httpContextAccessor;
            _productService = productService;
            _checkoutService = checkoutService;
            
        }
        public IActionResult ConfrimOrder()
        { 
            return View();
        }
        public IActionResult Checkout() 
        {
            if (HttpContext.Session.GetString(CDictionary.SK_LOINGED_USER) != null)
            {
                // 從 Session 中讀取抓到的 MEMBER ID
                var memberInfoJson = HttpContext.Session.GetString(CDictionary.SK_LOINGED_USER);
                var memberInfo = JsonSerializer.Deserialize<ShopMemberInfo>(memberInfoJson);

                // 使用 CheckoutService 來獲取可用的優惠券
                var usableCoupons = _checkoutService.GetUsableCoupons(memberInfo.MemberId);

                var viewModel = new CCheckoutViewModel
                {
                    memberUsableCoupon = usableCoupons ?? new List<CGetUsableCouponModel>(), // 初始化為空列表
                };

                return View(viewModel);
            }
            return View();
        }
        public IActionResult Cart()
        {
            
            string json = HttpContext.Session.GetString(CDictionary.SK_PURCHASED_PRODUCTS_LIST);
            if (json == null)
            {
                return View();
            }
            
            else
            {            
                List<CCartItem> cart = JsonSerializer.Deserialize<List<CCartItem>>(json);
                return View(cart);
            }
        }
    }
}
