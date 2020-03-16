using Microsoft.AspNetCore.Http;

namespace Cerberus.API.Extensions
{
	public static class HttpContextExtensions
	{
		public static bool IsAdmin(this HttpContext context)
		{
			if (context.User == null)
			{
				return false;
			}

			return context.User.IsInRole("admin") || context.User.IsInRole("cerberus-admin");
		}

		public static bool IsAuth(this HttpContext context, string securityToken)
		{
			var header = context.Request.Headers["SecurityHeader"].ToString();
			return securityToken == header;
		}
	}
}
