using System;

namespace IdentityServer4.Admin.DTO
{
	public class ListClientDTO
	{
		public Guid Id { get; set; }

		public string ClientId { get; set; }

		public string ClientName { get; set; }
		public string Scopes { get; set; }
		public string GrantType { get; set; }

		public string RedirectUris { get; set; }

		public bool Enabled { get; set; }
	}
}
