using System.Threading.Tasks;

namespace IdentityServer4.SecurityTokenService.Services
{
	public interface IEmailSender
	{
		Task SendEmailAsync(string email, string subject, string message);
	}
}
