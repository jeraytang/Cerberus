using System;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.SecurityTokenService.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
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

        public async Task InvokeAsync(HttpContext context, STSOptions _options)
        {
            var endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;
            if (endpoint != null && !string.IsNullOrWhiteSpace(_options.AccessDeniedPages))
            {
                var controllerName = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>()?.ControllerName;
                var actionName = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>()?.ActionName;
                var pages = _options.AccessDeniedPages.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.ToLower().Trim()).ToList();
                if (!string.IsNullOrEmpty(controllerName) && !string.IsNullOrEmpty(actionName))
                {
                    var route = $"{controllerName.ToLower()}/{actionName.ToLower()}";
                    if (pages.Contains(route))
                    {
                        context.Response.StatusCode = 404;
                        context.Response.Redirect("/");
                    }
                }
            }
            await _next(context);
        }
    }
}