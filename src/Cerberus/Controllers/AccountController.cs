using Microsoft.AspNetCore.Mvc;

namespace Cerberus.Controllers
{
	public class AccountController : Controller
	{
		public IActionResult Logout()
		{
			return SignOut("Cookies", "oidc");
		}
	}
}
