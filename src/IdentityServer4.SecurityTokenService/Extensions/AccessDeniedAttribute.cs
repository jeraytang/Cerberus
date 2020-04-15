using System;

namespace IdentityServer4.SecurityTokenService.Extensions
{
    /// <summary>
    /// 禁止访问特性-打上后直接跳转回主页
    /// </summary>
    public class AccessDeniedAttribute : Attribute
    {
        //标记
    }
}