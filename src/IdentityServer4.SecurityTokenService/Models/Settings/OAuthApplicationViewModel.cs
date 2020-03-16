using System;
using System.Collections.Generic;

namespace IdentityServer4.SecurityTokenService.Models.Settings
{
	public class OAuthApplicationViewModel
	{
		public IEnumerable<GrantViewModel> Applications { get; set; }
	}

	public class GrantViewModel
	{
		public string ClientId { get; set; }
		public string ClientName { get; set; }
		public string ClientUrl { get; set; }
		public string ClientLogoUrl { get; set; }
		public DateTime Created { get; set; }
		public DateTime? Expires { get; set; }
		public IEnumerable<string> IdentityGrantNames { get; set; }
		public IEnumerable<string> ApiGrantNames { get; set; }
	}
}
