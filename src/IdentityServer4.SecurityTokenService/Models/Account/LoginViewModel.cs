using System.Collections.Generic;
using System.Linq;

namespace IdentityServer4.SecurityTokenService.Models.Account
{
	public class LoginViewModel : LoginInputModel
	{
		public bool AllowRememberLogin { get; set; } = true;
		public bool EnableLocalLogin { get; set; } = true;

		public IEnumerable<ExternalProvider> ExternalProviders { get; set; } = Enumerable.Empty<ExternalProvider>();

		public IEnumerable<ExternalProvider> VisibleExternalProviders { get; set; } =
			Enumerable.Empty<ExternalProvider>();
	}
}
