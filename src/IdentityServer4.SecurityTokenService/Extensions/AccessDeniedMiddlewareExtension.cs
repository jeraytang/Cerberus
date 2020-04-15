using System;
using Microsoft.AspNetCore.Builder;
namespace IdentityServer4.SecurityTokenService.Extensions
{
    public static class AccessDeniedMiddlewareExtension
    {
        /// <summary>
        /// 打上AccessDeniedAttribute特性，则跳转回主页
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IApplicationBuilder UseAccessDenied(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<AccessDeniedMiddleware>();
        }
    }
}