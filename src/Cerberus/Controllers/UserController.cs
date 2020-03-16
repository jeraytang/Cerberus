using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cerberus.Controllers
{
	[Authorize(Roles = "cerberus-admin, admin")]
	[Route("users")]
	public class UserController : Controller
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

		[HttpGet("{id}/roles")]
		public IActionResult Role()
		{
			return View();
		}
	}
}
