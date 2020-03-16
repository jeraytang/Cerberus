using System.ComponentModel.DataAnnotations;

namespace Cerberus.API.DTO
{
	public class ServiceDTO
	{
		public string Id { get; set; }

		/// <summary>
		/// 名称
		/// </summary>
		[Required]
		[StringLength(100)]
		public string Name { get; set; }

		/// <summary>
		/// 安全验证码
		/// </summary>
		[Required]
		[StringLength(36)]
		public string SecurityToken { get; set; }

		/// <summary>
		/// 描述
		/// </summary>
		[StringLength(500)]
		public string Description { get; set; }
	}
}
