using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer4.Admin.Controllers
{
	[Authorize(Roles = "admin, cerberus-admin")]
	[Route("identityResources")]
	public class IdentityResourceController
		: Controller
	{
		[HttpGet]
		public IActionResult Index()
		{
			return View();
		}
	}
}
