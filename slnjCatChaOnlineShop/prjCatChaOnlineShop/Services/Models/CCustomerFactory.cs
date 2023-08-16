using Microsoft.Data.SqlClient;
using prjCatChaOnlineShop.IvanModels;

namespace prjCatChaOnlineShop.Services.Models
{
    public class CCustomerFactory
    {
        private List<ShopMemberInfo> queryBySql(string sql, List<SqlParameter> paras)
        {
            //創建一個空的ShopMemberInfo物件列表
            List<ShopMemberInfo> list = new List<ShopMemberInfo>();
            //創建SqlConnection 物件，並設定連接字串
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = @"Data Source=.;Initial Catalog=貓抓抓;Integrated Security=True;TrustServerCertificate=true";
            //開連接
            conn.Open();
            //創建SqlCommand 物件，用於執行SQL查詢
            SqlCommand cmd = new SqlCommand(sql, conn);
            //如果有參數，將參數添加到 SqlCommand 的參數集合中
            if (paras != null)
            {
                cmd.Parameters.AddRange(paras.ToArray());
            }
            //執行查詢並獲取 SqlDataReader
            SqlDataReader reader = cmd.ExecuteReader();
            //逐行讀取查詢結果
            while (reader.Read())
            {
                ShopMemberInfo x = new ShopMemberInfo();
                x.MemberId = (int)reader["MemberId"];
                x.Name = reader["Name"].ToString();
                x.Email = reader["Email"].ToString();
                x.Address = reader["address"].ToString();
                x.Password = reader["password"].ToString();
                x.LoyaltyPoints = (int)reader["LoyaltyPoints"];
                x.CatCoinQuantity = (int)reader["CatCoinQuantity"];
                //將ShopMemberInfo物件添加到列表中
                list.Add(x);
            }
            //關連接
            conn.Close();
            //返回包含查詢結果的ShopMemberInfo物件列表
            return list;
        }

        //查詢Email是否有在ShopMemberInfo裡面
        internal ShopMemberInfo? queryByEmail(string email)
        {
            // 定義SQL查詢字串，使用參數化查詢，以避免 SQL 注入攻擊
            string sql = "SELECT * FROM ShopMemberInfo WHERE Email=@K_FEMAIL";
            // 建立 SqlParameter 列表，用來儲存參數值
            List<SqlParameter> paras = new List<SqlParameter>();
            // 將 email 參數作為 SqlParameter 添加到 paras 列表中
            paras.Add(new SqlParameter("K_FEMAIL", email));
            // 使用 queryBySql 方法執行SQL查詢，並傳入查詢字串和參數列表
            List<ShopMemberInfo> list = queryBySql(sql, paras);
            // 檢查查詢結果
            if (list.Count == 0)
            {
                // 若查詢結果為空，則返回null表示找不到對應的客戶資訊
                return null;
            }
            else
            {   // 若查詢結果不為空，則返回第一個符合條件的客戶資訊
                return list[0]; 
            }

        }           
    }
}
