using System.ComponentModel.DataAnnotations;

namespace IdentityServer4.SecurityTokenService.Models.Settings
{
	public class AddPhoneNumberViewModel
	{
		[Required]
		[Phone]
		[Display(Name = "Phone number")]
		public string PhoneNumber { get; set; }
	}
}
