using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityServer4.SecurityTokenService.Common
{
	public class RegisterFilterAttribute : Attribute, IAsyncActionFilter
	{
		public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			if (context.ActionDescriptor is ControllerActionDescriptor descriptor)
			{
				if (descriptor.ControllerTypeInfo.GetCustomAttribute<RegisterFilterAttribute>() != null ||
				    descriptor.MethodInfo.GetCustomAttribute<RegisterFilterAttribute>() != null)
				{
					var options = context.HttpContext.RequestServices.GetRequiredService<STSOptions>();
					if (!options.OpenRegister)
					{
						context.Result = new NotFoundResult();
						return;
					}
				}
			}

			await next();
		}
	}
}
