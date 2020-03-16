using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer4.Admin.Controllers
{
	[Route("clients")]
	[Authorize(Roles = "admin, cerberus-admin")]
	public class ClientController
		: Controller
	{
		[HttpGet]
		public IActionResult Index()
		{
			return View();
		}

		[HttpGet("{clientId}")]
		public IActionResult Edit()
		{
			return View();
		}

		[HttpGet("{clientId}/secrets")]
		public IActionResult Secrets()
		{
			return View();
		}
	}
}
