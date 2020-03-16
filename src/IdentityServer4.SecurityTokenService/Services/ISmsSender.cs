using System.Threading.Tasks;

namespace IdentityServer4.SecurityTokenService.Services
{
	public interface ISmsSender
	{
		Task SendSmsAsync(string number, string message);
	}
}
