using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer4.Admin.Controllers
{
	[Authorize(Roles = "admin, cerberus-admin")]
	[Route("apiResources")]
	public class ApiResourceController
		: Controller
	{
		[HttpGet]
		public IActionResult Index()
		{
			return View();
		}

		[HttpGet("{apiResourceId}/scopes")]
		public IActionResult Scopes()
		{
			return View();
		}

		[HttpGet("{apiResourceId}/secrets")]
		public IActionResult Secrets()
		{
			return View();
		}
	}
}
