using System;

namespace IdentityServer4.Admin.DTO
{
	public class ListApiResourceDTO
	{
		public Guid Id { get; set; }

		/// <summary>
		/// Indicates if this resource is enabled. Defaults to true.
		/// </summary>
		public bool Enabled { get; set; }

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
	}
}
