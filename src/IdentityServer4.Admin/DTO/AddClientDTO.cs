using System.ComponentModel.DataAnnotations;

namespace IdentityServer4.Admin.DTO
{
	public class AddClientDTO
	{
		[Required]
		[StringLength(100)]
		public string ClientId { get; set; }

		[Required]
		[StringLength(100)]
		public string ClientName { get; set; }

		[StringLength(500)]
		public string Description { get; set; }

		[Required]
		public string AllowedGrantTypes { get; set; }

		[Required]
		[StringLength(500)]
		public string AllowedScopes { get; set; }
	}
}
