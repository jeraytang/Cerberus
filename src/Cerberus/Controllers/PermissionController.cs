using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cerberus.Controllers
{
	[Authorize(Roles = "cerberus-admin, admin")]
	[Route("permissions")]
	public class PermissionController : Controller
	{
		[HttpGet]
		public IActionResult Index()
		{
			return View();
		}
	}
}
