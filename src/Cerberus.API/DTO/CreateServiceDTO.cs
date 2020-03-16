using System.ComponentModel.DataAnnotations;

namespace Cerberus.API.DTO
{
	public class CreateServiceDTO
	{
		[Required]
		[StringLength(100)]
		public string Name { get; set; }

		/// <summary>
		/// 安全验证码
		/// </summary>
		[Required]
		[StringLength(36)]
		public string SecurityToken { get; set; }

		[StringLength(500)]
		public string Description { get; set; }
	}
}
