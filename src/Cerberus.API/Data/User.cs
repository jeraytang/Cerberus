using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using MSFramework.Data;
using MSFramework.Domain;

namespace Cerberus.API.Data
{
    public class User : IdentityUser, IDeletionAudited, ICreationAudited, IModificationAudited
    {
        /// <summary>
        /// 用户来源
        /// </summary>
        [StringLength(100)]
        public string Source { get; set; }

        /// <summary>
        /// Last modifier user for this entity.
        /// </summary>
        [StringLength(200)]
        [Description("最后修改者标识")]
        public string LastModificationUserId { get; set; }

        /// <summary>
        /// Last modifier user for this entity.
        /// </summary>
        [StringLength(200)]
        [Description("最后修改者名称")]
        public string LastModificationUserName { get; set; }

        /// <summary>
        /// The last modified time for this entity.
        /// </summary>
        [Description("最后修改时间")]
        public DateTimeOffset? LastModificationTime { get; set; }

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
        [StringLength(200)]
        [Description("创建用户标识")]
        public string CreationUserId { get; private set; }

        /// <summary>
        /// 创建用户名称
        /// </summary>
        [Required]
        [StringLength(200)]
        [Description("创建用户名称")]
        public string CreationUserName { get; private set; }

        /// <summary>
        /// 是否已经删除
        /// </summary>
        [Required]
        [Description("是否已经删除")]
        public bool IsDeleted { get; private set; }

        /// <summary>
        /// Which user deleted this entity?
        /// </summary>
        [StringLength(255)]
        [Description("删除者标识")]
        public string DeletionUserId { get; private set; }

        /// <summary>
        /// Which user deleted this entity?
        /// </summary>
        [StringLength(255)]
        [Description("删除者名称")]
        public string DeletionUserName { get; private set; }

        /// <summary>
        /// Deletion time of this entity.
        /// </summary>
        [Description("删除时间")]
        public DateTimeOffset? DeletionTime { get; set; }
        
        public virtual ICollection<IdentityUserClaim<string>> UserClaims { get; set; }

        public User()
        {
            CreationUserId = "System";
            CreationTime = DateTimeOffset.Now;
            CreationUserName = "System";
        }

        public virtual void SetCreationAudited(string userId, string userName, DateTimeOffset creationTime = default)
        {
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

        public void Delete(string userId, string userName, DateTimeOffset deletionTime = default)
        {
            // 删除只能一次操作，因此如果已经有值，不能再做设置
            if (!IsDeleted)
            {
                IsDeleted = true;

                if (DeletionTime == default)
                {
                    DeletionTime = deletionTime == default ? DateTimeOffset.Now : deletionTime;
                }

                if (!string.IsNullOrWhiteSpace(userId) && string.IsNullOrWhiteSpace(DeletionUserId))
                {
                    DeletionUserId = userId;
                }

                if (!string.IsNullOrWhiteSpace(userName) &&
                    string.IsNullOrWhiteSpace(DeletionUserName))
                {
                    DeletionUserName = userName;
                }
            }
        }

        public void Modify(string userName, string email, string phoneNumber, string source)
        {
            userName.NotNullOrWhiteSpace(nameof(userName));
            email.NotNullOrWhiteSpace(nameof(email));

            UserName = userName;
            NormalizedUserName = userName.ToUpper();
            Email = email;
            NormalizedEmail = email.ToUpper();
            PhoneNumber = phoneNumber;
            Source = source;
        }

        public bool Enabled
        {
            get
            {
                if (!LockoutEnabled)
                {
                    return true;
                }
                else
                {
                    if (LockoutEnd.HasValue)
                    {
                        return LockoutEnd.Value < DateTimeOffset.Now;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
        }
    }
}