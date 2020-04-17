using System;
using System.Collections.Generic;
using System.Linq;
using IdentityServer4.SecurityTokenService.Common;
using Microsoft.AspNetCore.Builder;

namespace IdentityServer4.SecurityTokenService.Extensions
{
    public static class AccessDeniedMiddlewareExtension
    {
        public static Dictionary<string, AccessDeniedPage> _accessDeniedPages =
            new Dictionary<string, AccessDeniedPage>();

        /// <summary>
        /// 打上AccessDeniedAttribute特性，则跳转回主页
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IApplicationBuilder UseAccessDenied(this IApplicationBuilder app,
            List<AccessDeniedPage> accessDeniedPages)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (accessDeniedPages != null)
            {
                _accessDeniedPages = accessDeniedPages.ToDictionary(
                    x =>
                        $"{x.HttpMethod.Trim().ToLower()}/{x.Controller.Trim().ToLower()}/{x.Action.Trim().ToLower()}");
            }

            return app.UseMiddleware<AccessDeniedMiddleware>();
        }
    }
}