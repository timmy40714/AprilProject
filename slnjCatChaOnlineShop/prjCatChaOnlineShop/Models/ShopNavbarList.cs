﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace prjCatChaOnlineShop.Models;

public partial class ShopNavbarList
{
    public int NavbarId { get; set; }

    public string NavbarText { get; set; }

    public bool? DisplayOrNot { get; set; }

    public DateTime? PublishTime { get; set; }

    public virtual ICollection<ShopNavbarChild> ShopNavbarChild { get; set; } = new List<ShopNavbarChild>();
}