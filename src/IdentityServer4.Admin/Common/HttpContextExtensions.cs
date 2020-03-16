using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace IdentityServer4.Admin.Common
{
	public static class HttpContextExtensions
	{
		public static string GetUserId(this HttpContext context)
		{
			if (context?.User == null)
			{
				return null;
			}

			var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (string.IsNullOrWhiteSpace(userId))
			{
				userId = context.User.FindFirst("sid")?.Value;
			}

			return userId;
		}

		public static string GetUserName(this HttpContext context)
		{
			if (context?.User == null)
			{
				return null;
			}

			var userName = context.User.FindFirst(ClaimTypes.Name)?.Value;
			if (string.IsNullOrWhiteSpace(userName))
			{
				userName = context.User.FindFirst("name")?.Value;
			}

			return userName;
		}
	}
}
