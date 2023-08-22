using Microsoft.AspNetCore.Mvc;
using prjCatChaOnlineShop.Models;
using prjCatChaOnlineShop.Models.CDictionary;
using prjCatChaOnlineShop.Models.ViewModels;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using Microsoft.Data.SqlClient;
using static prjCatChaOnlineShop.Models.ViewModels.CForgetPwdModel;
using System.Configuration;
using System.Data;
using Google.Apis.Auth;

namespace prjCatChaOnlineShop.Controllers.Home
{
    public class MemberLoginController : Controller
    {
        //將 _context 注入控制器，可以在控制器的操作方法中使用 _context 來執行資料庫查詢和操作
        private readonly cachaContext _context;
        private readonly IConfiguration _configuration;
        public MemberLoginController(cachaContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        //登入的方法
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(CLoginModel vm)
        {
            //確定有抓到全部會員的資料，若資料表信箱或密碼其中有空值就會跳例外錯誤
            ShopMemberInfo user = (new cachaContext()).ShopMemberInfo.FirstOrDefault(
                t => t.Email.Equals(vm.txtEmail) && t.Password.Equals(vm.txtPassword));
            if (user != null && user.Password.Equals(vm.txtPassword))
            {
                string Json = JsonSerializer.Serialize(user);
                HttpContext.Session.SetString(CDictionary.SK_LOINGED_USER, Json);
                return RedirectToAction("Index", "Index");
            }
            return Content("錯誤");
        }

        public IActionResult ForgetPassword()
        {
            return View();
        }

        public IActionResult RegisterMember()
        {
            return View();
        }
        [HttpPost]
        public IActionResult RegisterMember(ShopMemberInfo registerModel)
        {

            _context.ShopMemberInfo.Add(registerModel);
            _context.SaveChanges();

            return RedirectToAction("Login");

        }
        //驗證信箱是否存在
        [HttpPost]
        public JsonResult CheckEmailExist(string email)
        {
            bool emailExist = _context.ShopMemberInfo.Any(x => x.Email == email);
            return Json(emailExist);
        }

        //重設密碼
        public IActionResult ResetPassword(string verify)
        {
            if (string.IsNullOrEmpty(verify))
            {
                ViewData["ErrorMsg"] = "缺少驗證碼";
                return View();
            }

            // 取得系統自定密鑰，這裡使用 IConfiguration 讀取
            string secretKey = _configuration["SecretKey"];

            try
            {
                // 使用 AES 解密驗證碼
                string decryptedVerify = DecryptString(verify, secretKey);

                if (string.IsNullOrEmpty(decryptedVerify))
                {
                    ViewData["ErrorMsg"] = "驗證碼錯誤";
                    return View();
                }

                // 解析分隔的資料
                string[] verifyParts = decryptedVerify.Split('|');

                if (verifyParts.Length != 2)
                {
                    ViewData["ErrorMsg"] = "驗證碼格式不正確";
                    return View();
                }

                string userID = verifyParts[0];
                DateTime resetTime;

                if (!DateTime.TryParse(verifyParts[1], out resetTime))
                {
                    ViewData["ErrorMsg"] = "無效的重設時間";
                    return View();
                }

                // 檢查時間是否超過 30 分鐘
                TimeSpan timeElapsed = DateTime.Now - resetTime;

                if (timeElapsed.TotalMinutes > 30)
                {
                    ViewData["ErrorMsg"] = "超過驗證碼有效時間，請重寄驗證碼";
                    return View();
                }

                // 驗證碼檢查成功，加入 Session
                HttpContext.Session.SetString("ResetPwdUserId", userID);

                return View();
            }
            catch (Exception ex)
            {
                ViewData["ErrorMsg"] = "處理驗證碼時出錯";
                return View();
            }
        }

        private string DecryptString(string cipherText, string key)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key);
                aesAlg.Mode = CipherMode.ECB;
                aesAlg.Padding = PaddingMode.PKCS7;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            //// 使用 Google Mail Server 發信
            //string GoogleID = "smilehsiang@gmail.com"; //Google 發信帳號
            //string TempPwd = "wafdnrkmgrjagvvf"; //應用程式密碼
            //string ReceiveMail = "smilehsiang@yahoo.com.tw"; //接收信箱

            //string SmtpServer = "smtp.gmail.com";
            //int SmtpPort = 587;
            //MailMessage mms = new MailMessage();
            //mms.From = new MailAddress(GoogleID);
            //mms.Subject = "信件主題";
            //mms.Body = "信件內容";
            //mms.IsBodyHtml = true;
            //mms.SubjectEncoding = Encoding.UTF8;
            //mms.To.Add(new MailAddress(ReceiveMail));
            //using (SmtpClient client = new SmtpClient(SmtpServer, SmtpPort))
            //{
            //    client.EnableSsl = true;
            //    client.Credentials = new NetworkCredential(GoogleID, TempPwd);//寄信帳密 
            //    client.Send(mms); //寄出信件
            //}
        }


        /// <summary>
        /// 寄送驗證碼
        /// </summary>
        /// <returns></returns>
        
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public IActionResult SendMailToken(SendMailTokenIn inModel)
        {
            SendMailTokenOut outModel = new SendMailTokenOut();

            // 檢查輸入來源
            if (string.IsNullOrEmpty(inModel.MemberID))
            {
                outModel.ErrMsg = "請輸入帳號";
                return Json(outModel);
            }

            // 檢查資料庫是否有這個帳號
            string connStr = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // 取得會員資料
                string sql = "SELECT * FROM Member WHERE UserID = @UserID";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserID", inModel.MemberID);

                using (SqlDataAdapter adpt = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    adpt.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        string UserEmail = dt.Rows[0]["UserEmail"].ToString();

                        // 產生帳號+時間驗證碼
                        string sVerify = inModel.MemberID + "|" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

                        // 網站網址
                        string webPath = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.PathBase}";

                        // 從信件連結回到重設密碼頁面
                        string receivePage = "Member/ResetPwd";

                        // 信件內容
                        string mailContent = $"請點擊以下連結，返回網站重新設定密碼，逾期 30 分鐘後，此連結將會失效。<br><br><a href='{webPath}/{receivePage}?verify={sVerify}' target='_blank'>點此連結</a>";

                        // 信件主題
                        string mailSubject = "[測試] 重設密碼申請信";

                        // Google 發信帳號密碼
                        string GoogleMailUserID = _configuration["GoogleMailUserID"];
                        string GoogleMailUserPwd = _configuration["GoogleMailUserPwd"];

                        // 使用 Google Mail Server 發信
                        string SmtpServer = "smtp.gmail.com";
                        int SmtpPort = 587;
                        MailMessage mms = new MailMessage();
                        mms.From = new MailAddress(GoogleMailUserID);
                        mms.Subject = mailSubject;
                        mms.Body = mailContent;
                        mms.IsBodyHtml = true;
                        mms.SubjectEncoding = Encoding.UTF8;
                        mms.To.Add(new MailAddress(UserEmail));

                        using (SmtpClient client = new SmtpClient(SmtpServer, SmtpPort))
                        {
                            client.EnableSsl = true;
                            client.Credentials = new NetworkCredential(GoogleMailUserID, GoogleMailUserPwd);

                            client.Send(mms);
                        }

                        outModel.ResultMsg = "請於 30 分鐘內至你的信箱點擊連結重新設定密碼，逾期將無效";
                    }
                    else
                    {
                        outModel.ErrMsg = "查無此帳號";
                    }
                }
            }

            return Json(outModel);
        }



        /// <summary>
        /// 重設密碼
        /// </summary>
        /// <param name="inModel"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        public ActionResult DoResetPwd(DoResetPwdIn inModel)
        {
            DoResetPwdOut outModel = new DoResetPwdOut();

            // 檢查是否有輸入密碼
            if (string.IsNullOrEmpty(inModel.NewUserPwd))
            {
                outModel.ErrMsg = "請輸入新密碼";
                return Json(outModel);
            }
            if (string.IsNullOrEmpty(inModel.CheckUserPwd))
            {
                outModel.ErrMsg = "請輸入確認新密碼";
                return Json(outModel);
            }
            if (inModel.NewUserPwd != inModel.CheckUserPwd)
            {
                outModel.ErrMsg = "新密碼與確認新密碼不相同";
                return Json(outModel);
            }

            // 檢查帳號 Session 是否存在
            var resetPwdUserId = HttpContext.Session.GetString("ResetPwdUserId");
            if (string.IsNullOrEmpty(resetPwdUserId))
            {
                outModel.ErrMsg = "無修改帳號";
                return Json(outModel);
            }

            // 將新密碼使用 SHA256 雜湊運算(不可逆)
            string salt = resetPwdUserId.Substring(0, 1).ToLower(); // 使用帳號前一碼當作密碼鹽
            SHA256 sha256 = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(salt + inModel.NewUserPwd); // 將密碼鹽及新密碼組合
            byte[] hash = sha256.ComputeHash(bytes);
            string NewPwd = BitConverter.ToString(hash).Replace("-", "").ToLower(); // 雜湊運算後密碼

            // 取得連線字串
            string connStr = _configuration.GetConnectionString("CachaConnection");

            // 當程式碼離開 using 區塊時，會自動關閉連接
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // 資料庫連線
                conn.Open();

                // 修改個人資料至資料庫
                string sql = @"UPDATE Shop.MemberInfo SET Password = @UserPwd WHERE MemberID = @UserID";
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = sql;

                // 使用參數化填值
                cmd.Parameters.AddWithValue("@UserID", resetPwdUserId);
                cmd.Parameters.AddWithValue("@UserPwd", NewPwd);

                // 執行資料庫更新動作
                int Ret = cmd.ExecuteNonQuery();

                if (Ret > 0)
                {
                    outModel.ResultMsg = "重設密碼完成";
                }
                else
                {
                    outModel.ErrMsg = "無異動資料";
                }
            }

            // 回傳 Json 給前端
            return Json(outModel);
        }

        public IActionResult ValidGoogleLogin()
        {
            string? formCredential = Request.Form["credential"]; //回傳憑證
            string? formToken = Request.Form["g_csrf_token"]; //回傳令牌
            string? cookiesToken = Request.Cookies["g_csrf_token"]; //Cookie 令牌

            // 驗證 Google Token
            GoogleJsonWebSignature.Payload? payload = VerifyGoogleToken(formCredential, formToken, cookiesToken).Result;
            if (payload == null)
            {
                // 驗證失敗
                ViewData["Msg"] = "驗證 Google 授權失敗";
            }
            else
            {
                //驗證成功，取使用者資訊內容
                ViewData["Msg"] = "驗證 Google 授權成功" + "<br>";
                ViewData["Msg"] += "Email:" + payload.Email + "<br>";
                ViewData["Msg"] += "Name:" + payload.Name + "<br>";
                ViewData["Msg"] += "Picture:" + payload.Picture;
            }

            return View();
        }

        /// <summary>
        /// 驗證 Google Token
        /// </summary>
        /// <param name="formCredential"></param>
        /// <param name="formToken"></param>
        /// <param name="cookiesToken"></param>
        /// <returns></returns>
        public async Task<GoogleJsonWebSignature.Payload?> VerifyGoogleToken(string? formCredential, string? formToken, string? cookiesToken)
        {
            // 檢查空值
            if (formCredential == null || formToken == null && cookiesToken == null)
            {
                return null;
            }

            GoogleJsonWebSignature.Payload? payload;
            try
            {
                // 驗證 token
                if (formToken != cookiesToken)
                {
                    return null;
                }

                // 驗證憑證
                IConfiguration Config = new ConfigurationBuilder().AddJsonFile("appSettings.json").Build();
                string GoogleApiClientId = Config.GetSection("GoogleApiClientId").Value;
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string>() { GoogleApiClientId }
                };
                payload = await GoogleJsonWebSignature.ValidateAsync(formCredential, settings);
                if (!payload.Issuer.Equals("accounts.google.com") && !payload.Issuer.Equals("https://accounts.google.com"))
                {
                    return null;
                }
                if (payload.ExpirationTimeSeconds == null)
                {
                    return null;
                }
                else
                {
                    DateTime now = DateTime.Now.ToUniversalTime();
                    DateTime expiration = DateTimeOffset.FromUnixTimeSeconds((long)payload.ExpirationTimeSeconds).DateTime;
                    if (now > expiration)
                    {
                        return null;
                    }
                }
            }
            catch
            {
                return null;
            }
            return payload;
        }
    }
}
