using System;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;

namespace IdentityServer4.MySql.Services
{
	public class ProfileService : IProfileService
	{
		public string UserIdClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";

		public Task GetProfileDataAsync(ProfileDataRequestContext context)
		{
			var userId = GetUserId(context.Subject);

			//var user = await _userManager.GetUserAsync(context.Subject);

			// if (user == null)
			// {
			// 	throw new ArgumentException("User not exits");
			// }

			throw new NotImplementedException();
		}

		public Task IsActiveAsync(IsActiveContext context)
		{
			throw new NotImplementedException();
		}

		protected virtual string GetUserId(ClaimsPrincipal principal)
		{
			if (principal == null)
			{
				throw new ArgumentNullException(nameof(principal));
			}

			return principal.FindFirstValue(UserIdClaimType);
		}
	}
}
