﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace prjCatChaOnlineShop.Models;

public partial class GameCharacterInfo
{
    public byte[] MemberId { get; set; }

    public string CharacterName { get; set; }

    public string CharacterLevel { get; set; }

    public string CharacterEquipment { get; set; }

    public string CharacterItems { get; set; }

    public int GachaTicketQuantity { get; set; }

    public int GameCoinQuantity { get; set; }

    public int DiscountCoinQuantity { get; set; }
}