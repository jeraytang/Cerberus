using System.ComponentModel.DataAnnotations;

namespace IdentityServer4.SecurityTokenService.Models.Account
{
	public class UseRecoveryCodeViewModel
	{
		/// <summary>
		/// Recovery code
		/// </summary>
		[Required]
		public string Code { get; set; }

		public string ReturnUrl { get; set; }
	}
}
