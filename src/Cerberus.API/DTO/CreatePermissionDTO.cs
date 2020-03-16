using System.ComponentModel.DataAnnotations;
using Cerberus.API.Data;

namespace Cerberus.API.DTO
{
	public class CreatePermissionDTO
	{
		/// <summary>
		/// 权限名字
		/// </summary>
		[StringLength(255)]
		[Required]
		public string Name { get; set; }

		/// <summary>
		/// 权限类型
		/// </summary>
		[Required]
		public PermissionType Type { get; set; }

		/// <summary>
		/// 权限模块（分类）
		/// </summary>
		[StringLength(255)]
		public string Module { get; set; }

		/// <summary>
		/// 权限所属服务
		/// </summary>
		[StringLength(36)]
		[Required]
		public string ServiceId { get; set; }

		/// <summary>
		/// API 权限的路径
		/// </summary>
		[StringLength(255)]
		[Required]
		public string Identification { get; set; }

		/// <summary>
		/// 权限描述
		/// </summary>
		[StringLength(500)]
		public string Description { get; set; }
	}
}
