namespace prjCatChaOnlineShop.Models.ViewModels
{
    public class EcpayResponseModel
    {
        public string MerchantID { get; set; }
        public string MerchantTradeNo { get; set;}
        public string LogisticsSubType { get; set; }
        public string CVSStoreID { get; set;}
        public string CVSStoreName { get; set; }
        public string CVSAddress { get; set; }
        public string CVSTelephone { get; set;}
        public string CVSOutSide { get; set; }
        public string ExtraData { get; set; }

    }
}
