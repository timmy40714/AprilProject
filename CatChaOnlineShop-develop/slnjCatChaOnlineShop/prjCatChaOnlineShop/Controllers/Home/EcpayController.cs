using Day20.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using prjCatChaOnlineShop.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using XSystem.Security.Cryptography;



namespace prjCatChaOnlineShop.Controllers.Home
{
    public class EcpayController : Controller
    {
        private readonly IMemoryCache _memoryCache;
        private readonly cachaContext _context;
        public EcpayController(cachaContext context, IMemoryCache memoryCache)
        {
            _context = context;
            _memoryCache = memoryCache;
        }

        //step1 : 網頁導入傳值到前端
        public ActionResult Index()

        {

            var orderId = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
            //需填入你的網址
            var website = $"https://localhost:7218";
            var order = new Dictionary<string, string>
            {
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
                { "OrderResultURL", $"{website}/Home/PayInfo/{orderId}"},
                { "PaymentInfoURL",  $"{website}/Ecpay/AddAccountInfo"},
                { "ClientRedirectURL",  $"{website}/Home/AccountInfo/{orderId}"},
                { "MerchantID",  "2000132"},
                { "IgnorePayment",  "GooglePay#WebATM#CVS#BARCODE"},
                { "PaymentType",  "aio"},
                { "ChoosePayment",  "ALL"},
                { "EncryptType",  "1"},
            };
            //檢查碼
            order["CheckMacValue"] = GetCheckMacValue(order);
            return View(order);
        }
        /// step5 : 取得付款資訊，更新資料庫 OrderResultURL
        [HttpPost]
        public ActionResult PayInfo(FormCollection id)
        {
            var data = new Dictionary<string, string>();
            foreach (string key in id.Keys)
            {
                data.Add(key, id[key]);
            }
            var Orders = _context.EcpayOrders.ToList().Where(m => m.MerchantTradeNo == id["MerchantTradeNo"]).FirstOrDefault();
            Orders.RtnCode = int.Parse(id["RtnCode"]);
            if (id["RtnMsg"] == "Succeeded") Orders.RtnMsg = "已付款";
            Orders.PaymentDate = Convert.ToDateTime(id["PaymentDate"]);
            Orders.SimulatePaid = int.Parse(id["SimulatePaid"]);
            _context.SaveChanges();
            return View("EcpayView", data);

            //Database1Entities db = new Database1Entities();
            //var Orders = db.EcpayOrders.ToList().Where(m => m.MerchantTradeNo == id["MerchantTradeNo"]).FirstOrDefault();
            //Orders.RtnCode = int.Parse(id["RtnCode"]);
            //if (id["RtnMsg"] == "Succeeded") Orders.RtnMsg = "已付款";
            //Orders.PaymentDate = Convert.ToDateTime(id["PaymentDate"]);
            //Orders.SimulatePaid = int.Parse(id["SimulatePaid"]);
            //db.SaveChanges();
            //return View("EcpayView", data);
        }
        /// step5 : 取得虛擬帳號 資訊  ClientRedirectURL
        [HttpPost]
        public ActionResult AccountInfo(FormCollection id)
        {
            var data = new Dictionary<string, string>();
            foreach (string key in id.Keys)
            {
                data.Add(key, id[key]);
            }

            var Orders = _context.EcpayOrders.ToList().Where(m => m.MerchantTradeNo == id["MerchantTradeNo"]).FirstOrDefault();
            Orders.RtnCode = int.Parse(id["RtnCode"]);
            if (id["RtnMsg"] == "Succeeded") Orders.RtnMsg = "已付款";
            Orders.PaymentDate = Convert.ToDateTime(id["PaymentDate"]);
            Orders.SimulatePaid = int.Parse(id["SimulatePaid"]);
            _context.SaveChanges();
            return View("EcpayView", data);

            //Database1Entities db = new Database1Entities();
            //var Orders = db.EcpayOrders.ToList().Where(m => m.MerchantTradeNo == id["MerchantTradeNo"]).FirstOrDefault();
            //Orders.RtnCode = int.Parse(id["RtnCode"]);
            //if (id["RtnMsg"] == "Succeeded") Orders.RtnMsg = "已付款";
            //Orders.PaymentDate = Convert.ToDateTime(id["PaymentDate"]);
            //Orders.SimulatePaid = int.Parse(id["SimulatePaid"]);
            //db.SaveChanges();
            //return View("EcpayView", data);
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

            //var result = new StringBuilder();
            //var sha256 = SHA256Managed.Create();
            //var bts = Encoding.UTF8.GetBytes(value);
            //var hash = sha256.ComputeHash(bts);

            //for (int i = 0; i < hash.Length; i++)
            //{
            //    result.Append(hash[i].ToString("X2"));
            //}

            //return result.ToString();
        }

        //step4 : 新增訂單
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("Ecpay/AddOrders")]
        public string AddOrders(get_localStorage json)
        {
            string num = "0";
            try
            {
                EcpayOrders Orders = new EcpayOrders();
                Orders.MemberId = json.MerchantID;
                Orders.MerchantTradeNo = json.MerchantTradeNo;
                Orders.RtnCode = 0; //未付款
                Orders.RtnMsg = "訂單成功尚未付款";
                Orders.TradeNo = json.MerchantID.ToString();
                Orders.TradeAmt = json.TotalAmount;
                Orders.PaymentDate = Convert.ToDateTime(json.MerchantTradeDate);
                Orders.PaymentType = json.PaymentType;
                Orders.PaymentTypeChargeFee = "0";
                Orders.TradeDate = json.MerchantTradeDate;
                Orders.SimulatePaid = 0;
                _context.EcpayOrders.Add(Orders);
                _context.SaveChanges();
                num = "OK";
            }
            catch (Exception ex)
            {
                num = ex.ToString();
            }
            return num;
        }

        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("Ecpay/AddPayInfo")]

        // HomeController->Index->PaymentInfoURL所設定的
        public HttpResponseMessage AddPayInfo(JObject info)
        {
            try
            {

                _memoryCache.Set(info.Value<string>("MerchantTradeNo"), info, DateTime.Now.AddMinutes(60));
                return ResponseOK();
            }
            catch (Exception e)
            {
                return ResponseError();
            }
        }
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("Ecpay/AddAccountInfo")]
        public HttpResponseMessage AddAccountInfo(JObject info)
        {
            try
            {
                _memoryCache.Set(info.Value<string>("MerchantTradeNo"), info, DateTime.Now.AddMinutes(60));
                return ResponseOK();
            }
            catch (Exception e)
            {
                return ResponseError();
            }
        }
        private HttpResponseMessage ResponseError()
        {
            var response = new HttpResponseMessage();
            response.Content = new StringContent("0|Error");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }
        private HttpResponseMessage ResponseOK()
        {
            var response = new HttpResponseMessage();
            response.Content = new StringContent("1|OK");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }
    }

}
