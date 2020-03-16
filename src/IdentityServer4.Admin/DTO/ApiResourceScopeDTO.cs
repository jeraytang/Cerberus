using System;
using System.ComponentModel.DataAnnotations;

namespace IdentityServer4.Admin.DTO
{
	public class ApiResourceScopeDTO
	{
		public Guid Id { get; set; }

		/// <summary>
		/// Name of the scope. This is the value a client will use to request the scope.
		/// </summary>
		[Required]
		[StringLength(100)]
		public string Name { get; set; }

		/// <summary>
		/// Display name. This value will be used e.g. on the consent screen.
		/// </summary>
		[Required]
		[StringLength(100)]
		public string DisplayName { get; set; }

		/// <summary>
		/// Description. This value will be used e.g. on the consent screen.
		/// </summary>
		[StringLength(500)]
		public string Description { get; set; }

		/// <summary>
		/// Specifies whether the user can de-select the scope on the consent screen. Defaults to false.
		/// </summary>
		public string Required { get; set; }

		/// <summary>
		/// Specifies whether the consent screen will emphasize this scope. Use this setting for sensitive or important scopes. Defaults to false.
		/// </summary>
		public string Emphasize { get; set; }

		/// <summary>
		/// Specifies whether this scope is shown in the discovery document. Defaults to true.
		/// </summary>
		public string ShowInDiscoveryDocument { get; set; }

		[StringLength(500)] public string UserClaims { get; set; }
	}
}
