using System.ComponentModel.DataAnnotations;

namespace IdentityServer4.SecurityTokenService.Models.Settings
{
	public class ChangeUserNameViewModel
	{
		[Required]
		[StringLength(32, MinimumLength = 4)]
		public string NewUserName { get; set; }
	}
}
