using System;

namespace IdentityServer4.Storage.Adapter.Entities
{
	public class ClientCorsOrigins
	{
		/// <summary>
		/// 标识
		/// </summary>
		public Guid Id { get; set; } = Guid.NewGuid();

		public string Origin { get; set; }

		public Guid ClientId { get; set; }
	}
}
