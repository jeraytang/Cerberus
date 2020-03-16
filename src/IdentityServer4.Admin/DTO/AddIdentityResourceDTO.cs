using System.ComponentModel.DataAnnotations;

namespace IdentityServer4.Admin.DTO
{
	public class AddIdentityResourceDTO
	{
		/// <summary>
		/// Specifies whether the user can de-select the scope on the consent screen (if the consent screen wants to implement such a feature). Defaults to false.
		/// </summary>
		public bool Required { get; set; }

		/// <summary>
		/// Specifies whether the consent screen will emphasize this scope (if the consent screen wants to implement such a feature).
		/// Use this setting for sensitive or important scopes. Defaults to false.
		/// </summary>
		public bool Emphasize { get; set; }

		/// <summary>
		/// Specifies whether this scope is shown in the discovery document. Defaults to true.
		/// </summary>
		public bool ShowInDiscoveryDocument { get; set; }

		/// <summary>
		/// The unique name of the resource.
		/// </summary>
		[Required]
		[StringLength(100)]
		public string Name { get; set; }

		/// <summary>
		/// Display name of the resource.
		/// </summary>
		[Required]
		[StringLength(100)]
		public string DisplayName { get; set; }

		/// <summary>
		/// Description of the resource.
		/// </summary>
		[StringLength(500)]
		public string Description { get; set; }

		/// <summary>
		/// List of accociated user claims that should be included when this resource is requested.
		/// </summary>
		[Required]
		[StringLength(500)]
		public string UserClaims { get; set; }
	}
}
