using Microsoft.AspNetCore.Mvc;
using prjCatChaOnlineShop.Areas.AdminCMS.Models;
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
        private readonly cachaContext _context;
        private readonly CheckoutService _checkoutService;
        private readonly IWebHostEnvironment _host;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ProductService _productService;

        public CartController(cachaContext context, CheckoutService checkoutService, IWebHostEnvironment host, IHttpContextAccessor httpContextAccessor, ProductService productService)
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
            string loginUser = HttpContext.Session.GetString(CDictionary.SK_LOINGED_USER);
            string productList = HttpContext.Session.GetString(CDictionary.SK_PURCHASED_PRODUCTS_LIST);

            if (loginUser != null && productList != null)
            {
                // 從 Session 中讀取抓到的 MEMBER ID
                var memberInfoJson = HttpContext.Session.GetString(CDictionary.SK_LOINGED_USER);
                var memberInfo = JsonSerializer.Deserialize<ShopMemberInfo>(memberInfoJson);

                // 使用 CheckoutService 來獲取可用的優惠券
                var usableCoupons = _checkoutService.GetUsableCoupons(memberInfo.MemberId);
                // 使用 CheckoutService 來獲取儲存的地址
                var usableAddress = _checkoutService.GetUsableAddresses(memberInfo.MemberId);
                //購物車
                var cartItems = JsonSerializer.Deserialize<List<CCartItem>>(productList);

                var viewModel = new CCheckoutViewModel
                {
                    memberUsableCoupon = usableCoupons ?? new List<CGetUsableCouponModel>(), // 初始化為空列表
                    memberUsableAddress = usableAddress ?? new List<CgetUsableAddressModel>(),
                    cartItems = cartItems ?? new List<CCartItem>(),
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

        private int? GetCurrentMemberId()
        {
            var memberInfoJson = _httpContextAccessor.HttpContext?.Session.GetString(CDictionary.SK_LOINGED_USER);
            if (memberInfoJson != null)
            {
                var memberInfo = JsonSerializer.Deserialize<ShopMemberInfo>(memberInfoJson);
                return memberInfo.MemberId;
            }

            return null;
        }

        //[HttpPost]
        //public IActionResult AddOrder([FromBody] AddOrderViewModel orderData)
        //{
        //    try
        //    {
        //        var memberId = GetCurrentMemberId();

        //        if (memberId != null)
        //        {
        //            // 在這裡處理訂單創建邏輯
        //            ShopOrderTotalTable order = new ShopOrderTotalTable();
        //            order.MemberId = memberId;
        //            order.OrderCreationDate = DateTime.Now;
        //            order.OrderStatusId = 2;

        //            // 執行資料庫新增操作
        //            // 假設您使用 Entity Framework Core，可以像下面這樣執行新增操作
        //            using (var dbContext = new cachaContext()) // 替換成您的 DbContext
        //            {
        //                dbContext.ShopOrderTotalTable.Add(order);
        //                dbContext.SaveChanges();
        //            }

        //            // 此處您可以返回成功或其他所需的回應
        //            return Ok(new { message = "訂單已成功創建。" });
        //        }

        //        return BadRequest(new { message = "無法識別的會員 ID。" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "內部錯誤：" + ex.Message });
        //    }
        //}

    }
}
