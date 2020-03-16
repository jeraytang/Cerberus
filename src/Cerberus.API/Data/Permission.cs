using System.ComponentModel.DataAnnotations;
using MSFramework.Data;
using MSFramework.Domain;

namespace Cerberus.API.Data
{
	public class Permission : ModificationAuditedAggregateRoot<string>
	{
		/// <summary>
		/// 过期
		/// </summary>
		public bool Expired { get; private set; }

		/// <summary>
		/// 权限名字
		/// </summary>
		[StringLength(255)]
		public string Name { get; private set; }

		/// <summary>
		/// 权限类型
		/// </summary>
		public PermissionType Type { get; private set; }

		/// <summary>
		/// 权限模块（分类）
		/// </summary>
		[StringLength(255)]
		public string Module { get; private set; }

		/// <summary>
		/// API 权限的路径
		/// </summary>
		[StringLength(255)]
		public string Identification { get; private set; }

		/// <summary>
		/// 权限描述
		/// </summary>
		[StringLength(500)]
		public string Description { get; private set; }

		/// <summary>
		/// 权限所属服务
		/// </summary>
		[StringLength(36)]
		[Required]
		public string ServiceId { get; private set; }

		public virtual Service Service { get; private set; }

		private Permission()
		{
		}

		public Permission(string serviceId, PermissionType type, string module, string name,
			string identification,
			string description = "")
		{
			Modify(serviceId, type, module, name, identification, description);
		}

		public void Modify(string serviceId, PermissionType type, string module, string name,
			string identification,
			string description = "")
		{
			name.NotNullOrWhiteSpace(nameof(name));
			serviceId.NotNullOrWhiteSpace(nameof(serviceId));

			if (string.IsNullOrWhiteSpace(identification))
			{
				identification = name;
			}

			Type = type;
			Name = name;
			Module = module;
			Identification = identification.ToLower();
			Description = description;
			ServiceId = serviceId;
		}

		public void Expire()
		{
			Expired = true;
		}

		public void Renewal()
		{
			Expired = false;
		}

		//
		// public static string ComputeHash(string serviceId, string identification)
		// {
		// 	return $"{serviceId}-{identification}".ToMd5();
		// }
	}
}
