using Microsoft.AspNetCore.Mvc;

namespace IdentityServer4.Admin.Controllers
{
	public class AccountController : Controller
	{
		public IActionResult Logout()
		{
			return SignOut("Cookies", "oidc");
		}
	}
}
