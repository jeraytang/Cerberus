using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace IdentityServer4.SecurityTokenService.Extensions
{
    public class AccessDeniedMiddleware
    {
        private readonly RequestDelegate _next;

        public AccessDeniedMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;
            var accessDeniedAttribute = endpoint?.Metadata.GetMetadata<AccessDeniedAttribute>();
            if (accessDeniedAttribute != null)
            {
                //标记禁止访问的特性-跳转主页
                context.Response.StatusCode = 403;
                context.Response.Redirect("/");
            }

            await _next(context);
        }
    }
}