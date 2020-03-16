using System;
using System.Threading.Tasks;
using IdentityServer4.Storage.Adapter.Entities;

namespace IdentityServer4.Storage.Adapter.Repository
{
	public interface IIdentityResourceRepository
	{
		Task<int> AddAsync(params IdentityResource[] identityResource);
		Task<bool> UpdateAsync(IdentityResource identityResource);
		Task<bool> DeleteAsync(Guid id);
		Task<bool> ExistsAsync(string identity);
		Task<PagedQueryResult<IdentityResource>> PagedQueryAsync(int page, int limit);
		Task<bool> EnableAsync(Guid id);
		Task<bool> DisableAsync(Guid id);
		Task<IdentityResource> GetAsync(Guid id);
	}
}
