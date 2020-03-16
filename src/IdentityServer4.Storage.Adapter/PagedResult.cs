using System.Collections.Generic;

namespace IdentityServer4.Storage.Adapter
{
	public class PagedQueryResult<TEntity>
	{
		public int Count { get; set; }

		public int Page { get; set; }

		public int Limit { get; set; }

		public List<TEntity> Entities { get; set; }
	}
}
