using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cerberus.Controllers
{
	[Authorize(Roles = "cerberus-admin, admin")]
	[Route("roles")]
	public class RoleController : Controller
	{
		[HttpGet]
		public IActionResult Index()
		{
			return View();
		}

		[HttpGet("{id}/permissions")]
		public IActionResult Permission()
		{
			return View();
		}
	}
}
