using System.ComponentModel.DataAnnotations;
using MSFramework.Data;

namespace Cerberus.API.Data
{
    public class UserPermission
    {
        /// <summary>
        /// 用户标识
        /// </summary>
        [StringLength(36)]
        public string UserId { get; private set; }

        // /// <summary>
        // /// 用户
        // /// </summary>
        // public virtual User User { get; private set; }

        /// <summary>
        /// 权限标识
        /// </summary>
        [StringLength(36)]
        public string PermissionId { get; private set; }

        /// <summary>
        /// 权限元数据
        /// </summary>
        [StringLength(2000)]
        public string Data { get; private set; }

        private UserPermission()
        {
        }

        public UserPermission(string userId, string permissionId, string data)
        {
            userId.NotNullOrWhiteSpace(nameof(userId));
            permissionId.NotNullOrWhiteSpace(nameof(permissionId));

            UserId = userId;
            PermissionId = permissionId;
            Data = data;
        }
    }
}