namespace prjCatChaOnlineShop.Models.ViewModels
{
    public class AddOrderViewModel
    {
        public int MemberId { get; set; } // 會員 ID
        public DateTime OrderCreationDate { get; set; } // 訂單創建日期
        public int OrderStatusId { get; set; } // 訂單狀態 ID
                                              
    }
}
