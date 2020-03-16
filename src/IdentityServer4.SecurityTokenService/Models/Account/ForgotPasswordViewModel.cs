using System.ComponentModel.DataAnnotations;

namespace IdentityServer4.SecurityTokenService.Models.Account
{
	public class ForgotPasswordViewModel
	{
		/// <summary>
		/// Email
		/// </summary>
		[Required]
		[EmailAddress]
		public string Email { get; set; }
	}
}
