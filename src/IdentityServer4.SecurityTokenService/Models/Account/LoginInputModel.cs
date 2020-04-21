using System.ComponentModel.DataAnnotations;

namespace IdentityServer4.SecurityTokenService.Models.Account
{
    public class LoginInputModel
    {
        [Required(ErrorMessage = "请输入账号")] public string Username { get; set; }

        [Required(ErrorMessage = "请输入密码")] public string Password { get; set; }

        public bool RememberLogin { get; set; }

        public string ReturnUrl { get; set; }
    }
}