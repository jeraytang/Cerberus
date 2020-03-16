using System.ComponentModel.DataAnnotations;

namespace IdentityServer4.SecurityTokenService.Models.Account
{
	public class VerifyCodeViewModel
	{
		/// <summary>
		/// Provider
		/// </summary>
		[Required]
		public string Provider { get; set; }

		/// <summary>
		/// Verify code
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
