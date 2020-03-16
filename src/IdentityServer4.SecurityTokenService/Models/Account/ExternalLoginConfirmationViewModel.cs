using System.ComponentModel.DataAnnotations;

namespace IdentityServer4.SecurityTokenService.Models.Account
{
	public class ExternalLoginConfirmationViewModel
	{
		[Required]
		[EmailAddress]
		[StringLength(24, MinimumLength = 6)]
		public string Email { get; set; }
	}
}
