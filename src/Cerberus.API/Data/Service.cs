using System.ComponentModel.DataAnnotations;
using MSFramework.Data;
using MSFramework.Domain;

namespace Cerberus.API.Data
{
	public class Service : ModificationAuditedAggregateRoot<string>
	{
		/// <summary>
		/// 租户名称
		/// </summary>
		[Required]
		[StringLength(100)]
		public string Name { get; private set; }

		/// <summary>
		/// 安全验证码
		/// </summary>
		[Required]
		[StringLength(36)]
		public string SecurityToken { get; private set; }

		/// <summary>
		/// 租户描述
		/// </summary>
		[StringLength(500)]
		public string Description { get; private set; }

		public Service(string name, string securityToken, string description = "")
		{
			Modify(name, securityToken, description);
		}

		public void Modify(string name, string securityToken, string description)
		{
			name.NotNullOrWhiteSpace(nameof(name));
			securityToken.NotNullOrWhiteSpace(nameof(securityToken));
			Name = name;
			Description = description;
			SecurityToken = securityToken;
		}
	}
}
