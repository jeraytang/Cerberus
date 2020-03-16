using System.ComponentModel.DataAnnotations;

namespace IdentityServer4.SecurityTokenService.Models.Account
{
	public class VerifyAuthenticatorCodeViewModel
	{
		/// <summary>
		/// Authenticator code
		/// </summary>
		[Required]
		public string Code { get; set; }

		public string ReturnUrl { get; set; }

		[Display(Name = "Remember this browser?")]
		public bool RememberBrowser { get; set; }

		/// <summary>
		///
		/// </summary>
		[Display(Name = "Remember me?")]
		public bool RememberMe { get; set; }
	}
}
