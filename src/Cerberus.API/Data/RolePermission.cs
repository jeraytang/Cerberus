using System.ComponentModel.DataAnnotations;
using MSFramework.Data;

namespace Cerberus.API.Data
{
	public class RolePermission
	{
		/// <summary>
		///
		/// </summary>
		[StringLength(36)]
		public string RoleId { get; private set; }

		// /// <summary>
		// /// 角色
		// /// </summary>
		// public virtual Role Role { get; private set; }

		/// <summary>
		///
		/// </summary>
		[StringLength(36)]
		public string PermissionId { get; private set; }

		// /// <summary>
		// /// 权限
		// /// </summary>
		// public virtual Permission Permission { get; private set; }

		private RolePermission()
		{
		}

		public RolePermission(string roleId, string permissionId)
		{
			roleId.NotNullOrWhiteSpace(nameof(roleId));
			permissionId.NotNullOrWhiteSpace(nameof(permissionId));

			RoleId = roleId;
			PermissionId = permissionId;
		}
	}
}
