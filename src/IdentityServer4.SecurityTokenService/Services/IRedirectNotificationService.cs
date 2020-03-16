using IdentityServer4.SecurityTokenService.Models;

namespace IdentityServer4.SecurityTokenService.Services
{
	public interface IRedirectNotificationService
	{
		NotificationViewModel Receive(string location);
		void Send(string to, NotificationViewModel viewModel);
	}
}
