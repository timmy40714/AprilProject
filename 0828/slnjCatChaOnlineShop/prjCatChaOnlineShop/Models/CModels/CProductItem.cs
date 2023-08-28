﻿namespace prjCatChaOnlineShop.Models.CModels
{
    public class CProductItem
    {
        public ShopProductTotal? pItems { get; set; }
        public int pId 
        {
            get {return pItems.ProductId; }
            set { pItems.ProductId = value; }
        }
        public string? pName
        {
            get { return pItems.ProductName; }
            set { pItems.ProductName = value; }
        }
        public decimal? pDiscount
        {
            get { return pItems.Discount; }
            set { pItems.Discount = value; }
        }

        public decimal? pPrice
        {
            get { return pItems.ProductPrice; }
            set { pItems.ProductPrice = value; }
        }

        public int? pCategoryId
        {
            get { return pItems.ProductCategoryId; }
            set { pItems.ProductCategoryId = value; }
        }
        public string? pCategoryName
        {
            get;
            set;
        }
        public string? pCategoryImg
        {
            get;
            set;
        }
        public DateTime? p上架時間
        {
            get { return pItems.ReleaseDate; }
            set { pItems.ReleaseDate = value; }
        }

        public int? p剩餘庫存
        {
            get { return pItems.RemainingQuantity; }
            set { pItems.RemainingQuantity = value; }
        }
        public string? p商品描述
        {
            get { return pItems.ProductDescription; }
            set { pItems.ProductDescription = value; }
        }
        public string? p子項目
        {
            get { return pItems.Attributes; }
            set { pItems.Attributes = value; }
        }
        public decimal? p優惠價格
        {
            get
            {
                if (pItems.Discount < 0)//折數
                    return this.pItems.ProductPrice * this.pItems.Discount;
                else//折扣金額
                    return this.pItems.ProductPrice - this.pItems.Discount;
            }
        }
        public string? p圖片路徑 { get; set; }
    }
}
