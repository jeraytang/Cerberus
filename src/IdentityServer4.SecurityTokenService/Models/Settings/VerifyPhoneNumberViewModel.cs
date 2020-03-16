using System.ComponentModel.DataAnnotations;

namespace IdentityServer4.SecurityTokenService.Models.Settings
{
	public class VerifyPhoneNumberViewModel
	{
		/// <summary>
		/// Verify code
		/// </summary>
		[Required]
		public string Code { get; set; }

		[Required]
		[Phone]
		[Display(Name = "Phone number")]
		public string PhoneNumber { get; set; }
	}
}
