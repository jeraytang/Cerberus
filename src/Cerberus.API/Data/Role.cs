using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using MSFramework.Data;
using MSFramework.Domain;

namespace Cerberus.API.Data
{
	public class Role : IdentityRole, IModificationAudited, ICreationAudited
	{
		public Role(string name, string type, string description = "")
		{
			Modify(name, type, description);
		}

		/// <summary>
		/// 角色类型
		/// </summary>
		[StringLength(50)]
		public string Type { get; private set; }

		/// <summary>
		/// 角色描述
		/// </summary>
		[StringLength(500)]
		public string Description { get; private set; }

		/// <summary>
		/// Last modifier user for this entity.
		/// </summary>
		[StringLength(255)]
		[Description("最后修改者标识")]
		public string LastModificationUserId { get; private set; }

		/// <summary>
		/// Last modifier user for this entity.
		/// </summary>
		[StringLength(255)]
		[Description("最后修改者名称")]
		public string LastModificationUserName { get; private set; }

		/// <summary>
		/// The last modified time for this entity.
		/// </summary>
		[Description("最后修改时间")]
		public DateTimeOffset? LastModificationTime { get; private set; }

		/// <summary>
		/// 创建时间
		/// </summary>
		[Required]
		[Description("创建时间")]
		public DateTimeOffset CreationTime { get; private set; }

		/// <summary>
		/// 创建用户标识
		/// </summary>
		[Required]
		[StringLength(255)]
		[Description("创建用户标识")]
		public string CreationUserId { get; private set; }

		/// <summary>
		/// 创建用户名称
		/// </summary>
		[Required]
		[StringLength(255)]
		[Description("创建用户名称")]
		public string CreationUserName { get; private set; }

		public virtual void SetCreationAudited(string userId, string userName, DateTimeOffset creationTime = default)
		{
			// 创建只能一次操作，因此如果已经有值，不能再做设置
			if (CreationTime == default)
			{
				CreationTime = creationTime == default ? DateTimeOffset.Now : creationTime;
			}

			if (!string.IsNullOrWhiteSpace(userId) && string.IsNullOrWhiteSpace(CreationUserId))
			{
				CreationUserId = userId;
			}

			if (!string.IsNullOrWhiteSpace(userName) && string.IsNullOrWhiteSpace(CreationUserName))
			{
				CreationUserName = userName;
			}
		}

		public virtual void SetModificationAudited(string userId, string userName,
			DateTimeOffset lastModificationTime = default)
		{
			LastModificationTime = lastModificationTime == default ? DateTimeOffset.Now : lastModificationTime;

			if (!string.IsNullOrWhiteSpace(userId))
			{
				LastModificationUserId = userId;
			}

			if (!string.IsNullOrWhiteSpace(userName))
			{
				LastModificationUserName = userName;
			}
		}

		public void Modify(string name, string type, string description)
		{
			name.NotNullOrWhiteSpace(nameof(name));

			if (string.IsNullOrWhiteSpace(type))
			{
				type = "Role";
			}

			Name = name;
			Type = type;
			NormalizedName = name.ToUpper();
			Description = description;
		}
	}
}
