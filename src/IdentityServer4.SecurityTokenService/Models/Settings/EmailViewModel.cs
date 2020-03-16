using System.ComponentModel.DataAnnotations;

namespace IdentityServer4.SecurityTokenService.Models.Settings
{
	public class EmailViewModel
	{
		[EmailAddress]
		public string Email { get; set; }

		public bool EmailConfirmed { get; set; }
	}
}
