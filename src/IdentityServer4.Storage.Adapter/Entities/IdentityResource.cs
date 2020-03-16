namespace IdentityServer4.Storage.Adapter.Entities
{
	public class IdentityResource : EntityBase
	{
		/// <summary>
		/// Specifies whether the user can de-select the scope on the consent screen (if the consent screen wants to implement such a feature). Defaults to false.
		/// </summary>
		public bool Required { get; set; } = false;

		/// <summary>
		/// Specifies whether the consent screen will emphasize this scope (if the consent screen wants to implement such a feature).
		/// Use this setting for sensitive or important scopes. Defaults to false.
		/// </summary>
		public bool Emphasize { get; set; } = false;

		/// <summary>
		/// Specifies whether this scope is shown in the discovery document. Defaults to true.
		/// </summary>
		public bool ShowInDiscoveryDocument { get; set; } = true;

		/// <summary>
		/// Indicates if this resource is enabled. Defaults to true.
		/// </summary>
		public bool Enabled { get; set; } = true;

		/// <summary>
		/// The unique name of the resource.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Display name of the resource.
		/// </summary>
		public string DisplayName { get; set; }

		/// <summary>
		/// Description of the resource.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// List of accociated user claims that should be included when this resource is requested.
		/// </summary>
		public string UserClaims { get; set; }

		/// <summary>
		/// Gets or sets the custom properties for the resource.
		/// </summary>
		/// <value>
		/// The properties.
		/// </value>
		public string Properties { get; set; }
	}
}
