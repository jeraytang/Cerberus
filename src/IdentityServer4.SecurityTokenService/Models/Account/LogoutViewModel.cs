namespace IdentityServer4.SecurityTokenService.Models.Account
{
	public class LogoutViewModel : LogoutInputModel
	{
		public bool ShowLogoutPrompt { get; set; } = true;
	}
}
