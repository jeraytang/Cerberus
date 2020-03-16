using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("IdentityServer4.Storage.MySql")]
namespace IdentityServer4.Storage.Adapter.Entities
{
    public abstract class EntityBase
    {
        /// <summary>
        /// 标识
        /// </summary>
        public Guid Id { get; private set; } = Guid.NewGuid();

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTimeOffset CreationTime { get; private set; }

        /// <summary>
        /// 创建用户标识
        /// </summary>
        public string CreationUserId { get; private set; }

        /// <summary>
        /// 创建用户名称
        /// </summary>
        public string CreationUserName { get; private set; }

        /// <summary>
        /// Last modifier user for this entity.
        /// </summary>
        public string LastModificationUserId { get; private set; }

        /// <summary>
        /// Last modifier user for this entity.
        /// </summary>
        public string LastModificationUserName { get; private set; }

        /// <summary>
        /// The last modified time for this entity.
        /// </summary>
        public DateTimeOffset? LastModificationTime { get; private set; }

        internal void SetId(Guid id)
        {
            Id = id;
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
    }
}