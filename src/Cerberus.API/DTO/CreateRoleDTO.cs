using System.ComponentModel.DataAnnotations;

namespace Cerberus.API.DTO
{
	public class CreateRoleDTO
	{
		[Required]
		[StringLength(100)]
		public string Name { get; set; }

		[StringLength(50)]
		public string Type { get; set; }

		[StringLength(500)]
		public string Description { get; set; }
	}
}
