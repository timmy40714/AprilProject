﻿using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace prjCatChaOnlineShop.Models.ViewModels
{
    public class CLoginModel
    {
        public string txtEmail { get; set; }
        public string txtPassword { get; set; }
        public bool rememberMe { get; set; }
    }
}
