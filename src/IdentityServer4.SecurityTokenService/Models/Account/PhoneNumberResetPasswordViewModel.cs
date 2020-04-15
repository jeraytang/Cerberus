﻿using System.ComponentModel.DataAnnotations;

namespace IdentityServer4.SecurityTokenService.Models.Account
{
    public class PhoneNumberResetPasswordViewModel
    {
        [Display(Name = "verify code")]
        [Required(ErrorMessage = "请输入验证码")]
        [StringLength(6,ErrorMessage = "请输入6位验证码")]
        public string Code { get; set; }

        [Required(ErrorMessage = "请输入手机号码")]
        [Phone]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "请输入新密码")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "请再次输入新密码")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "密码和确认密码不一致！")]
        public string PasswordConfirm { get; set; }
    }
}