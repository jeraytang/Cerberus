using System.ComponentModel.DataAnnotations;

namespace Cerberus.API.ViewObject
{
    public class UserChangePasswordModel
    {
        [DataType(DataType.Password)]
        [Display(Name = "旧密码")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "新密码")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "确认新密码")]
        [Compare("NewPassword", ErrorMessage = "确认新密码与新密码不一致！")]
        public string ConfirmPassword { get; set; }
    }
}