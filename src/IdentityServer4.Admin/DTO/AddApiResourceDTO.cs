using System.ComponentModel.DataAnnotations;

namespace IdentityServer4.Admin.DTO
{
	public class AddApiResourceDTO
	{
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

		[StringLength(500)]
		public string UserClaims { get; set; }
	}
}
