using System.ComponentModel.DataAnnotations;
using MSFramework.Data;

namespace Cerberus.API.Data
{
	public class UserPermission
	{
		/// <summary>
		/// 用户标识
		/// </summary>
		[StringLength(40)]
		public string UserId { get; private set; }

		/// <summary>
		/// 用户
		/// </summary>
		public virtual User User { get; private set; }

		/// <summary>
		/// 权限标识
		/// </summary>
		[StringLength(40)]
		public string PermissionId { get; private set; }

		/// <summary>
		/// 权限
		/// </summary>
		public virtual Permission Permission { get; private set; }

		private UserPermission()
		{
		}

		public UserPermission(string userId, string permissionId)
		{
			userId.NotNullOrWhiteSpace(nameof(userId));
			permissionId.NotNullOrWhiteSpace(nameof(permissionId));

			UserId = userId;
			PermissionId = permissionId;
		}
	}
}
