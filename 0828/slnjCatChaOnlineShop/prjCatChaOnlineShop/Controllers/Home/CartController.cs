using Microsoft.AspNetCore.Mvc;
using prjCatChaOnlineShop.Areas.AdminCMS.Models;
using prjCatChaOnlineShop.Models;
using prjCatChaOnlineShop.Models.CDictionary;
using prjCatChaOnlineShop.Models.CModels;
using prjCatChaOnlineShop.Models.ViewModels;
using prjCatChaOnlineShop.Services.Function;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;

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
        //public IActionResult Checkout()
        //{
        //    return View();
        //}
      
        [HttpPost]
        public async Task<IActionResult> ReceiveHtml()
        {
            using (StreamReader reader = new StreamReader(Request.Body))
            {
                string htmlContent = await reader.ReadToEndAsync();

                // 在这里处理HTML内容，比如解析、存储到数据库等
                // 返回响应
                return Content("HTML content received and processed.");
            }
        }

   
        public IActionResult Checkout()
        {
            //using (StreamReader reader = new StreamReader(Request.Body))
            //{
            //    string htmlContent = reader.ReadToEnd();
            //}
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

                //創建綠界訂單
                var orderId = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
                //需填入你的網址
                var website = $"https://localhost:7218";
                var order = new Dictionary<string, string>
          {
            //綠界需要的參數
            { "MerchantTradeNo",  orderId},
            { "MerchantTradeDate",  DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")},
            { "TotalAmount",  "100"},
            { "TradeDesc",  "無"},
            { "ItemName",  "測試商品"},
            { "ExpireDate",  "3"},
            { "CustomField1",  ""},
            { "CustomField2",  ""},
            { "CustomField3",  ""},
            { "CustomField4",  ""},
            { "ReturnURL",  $"{website}/Ecpay/AddPayInfo"},
            { "OrderResultURL", $"{website}/EcpayHome/PayInfo/{orderId}"},
            { "PaymentInfoURL",  $"{website}/Ecpay/AddAccountInfo"},
            { "ClientRedirectURL",  $"{website}/EcpayHome/AccountInfo/{orderId}"},
            { "MerchantID",  "2000132"},
            { "IgnorePayment",  "GooglePay#WebATM#CVS#BARCODE"},
            { "PaymentType",  "aio"},
            { "ChoosePayment",  "ALL"},
            { "EncryptType",  "1"},
          };
                //檢查碼
                order["CheckMacValue"] = GetCheckMacValue(order);

                var viewModel = new CCheckoutViewModel
                {
                    memberUsableCoupon = usableCoupons ?? new List<CGetUsableCouponModel>(), // 初始化為空列表
                    memberUsableAddress = usableAddress ?? new List<CgetUsableAddressModel>(),
                    cartItems = cartItems ?? new List<CCartItem>(),
                    keyValuePairs=order ?? new Dictionary<string, string>()
                };

                return View(viewModel);
            }
           
           return View();
        }
        private string GetCheckMacValue(Dictionary<string, string> order)
        {
            var param = order.Keys.OrderBy(x => x).Select(key => key + "=" + order[key]).ToList();
            var checkValue = string.Join("&", param);
            //測試用的 HashKey
            var hashKey = "5294y06JbISpM5x9";
            //測試用的 HashIV
            var HashIV = "v77hoKGq4kWxNNIS";
            checkValue = $"HashKey={hashKey}" + "&" + checkValue + $"&HashIV={HashIV}";
            checkValue = HttpUtility.UrlEncode(checkValue).ToLower();
            checkValue = GetSHA256(checkValue);
            return checkValue.ToUpper();
        }
        private string GetSHA256(string value)
        {
            // EncryptType CheckMacValue加密類型
            var result = new StringBuilder();
            var sha256 = SHA256.Create();
            var bts = Encoding.UTF8.GetBytes(value);
            var hash = sha256.ComputeHash(bts);
            for (int i = 0; i < hash.Length; i++)
            {
                result.Append(hash[i].ToString("X2"));
            }
            return result.ToString();
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

        [HttpPost]
        public IActionResult AddOrder([FromBody] AddOrderViewModel orderData)
        {
            try
            {
                var memberId = GetCurrentMemberId();

                if (memberId != null)
                {
                    // 在這裡處理訂單創建邏輯
                    ShopOrderTotalTable order = new ShopOrderTotalTable();
                    order.MemberId = memberId;
                    order.OrderCreationDate = DateTime.Now;
                    order.OrderStatusId = 2;

                    // 執行資料庫新增操作
                    using (var dbContext = new cachaContext()) // 替換成您的 DbContext
                    {
                        dbContext.ShopOrderTotalTable.Add(order);
                        dbContext.SaveChanges();
                    }

                    // 此處您可以返回成功或其他所需的回應
                    return Ok(new { message = "訂單已成功創建。" });
                }

                return BadRequest(new { message = "無法識別的會員 ID。" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "內部錯誤：" + ex.Message });
            }
        }

    }
}
