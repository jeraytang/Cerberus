namespace IdentityServer4.Storage.Adapter.Entities
{
	public class ApiResource : EntityBase
	{
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

		public string UserClaims { get; set; }

		public string ApiSecrets { get; set; }

		public string Properties { get; set; }
	}
}
