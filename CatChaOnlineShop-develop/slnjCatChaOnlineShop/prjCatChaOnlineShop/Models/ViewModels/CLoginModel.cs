using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace prjCatChaOnlineShop.Models.ViewModels
{
    public class CLoginModel
    {
        public int MemberID { get; set; }
        public string txtEmail { get; set; }
        public string txtPassword { get; set; }
        public bool rememberMe { get; set; }
    }
}
