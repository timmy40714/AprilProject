﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace prjCatChaOnlineShop.IvanModels;

public partial class ShopFavoriteDataTable
{
    public int? MemberId { get; set; }

    public int FavoriteId { get; set; }

    public int? ProductId { get; set; }

    public DateTime? CreationDate { get; set; }

    public virtual ShopMemberInfo Member { get; set; }

    public virtual ShopProductTotal Product { get; set; }
}