using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Storage.Adapter.Entities;

namespace IdentityServer4.Storage.Adapter.Repository
{
	public interface IApiResourceRepository
	{
		Task<ApiResource> GetAsync(Guid id);
		Task<int> AddAsync(params ApiResource[] apiResource);
		Task<bool> UpdateAsync(ApiResource apiResource);
		Task<bool> DeleteAsync(Guid apiResourceId);
		Task<int> AddScopeAsync(params ApiScope[] scopes);
		Task<bool> UpdateScopeAsync(ApiScope scope);
		Task<bool> DeleteScopeAsync(Guid resourceId, Guid scopeId);

		Task<List<ApiScope>> GetScopesAsync(Guid apiResourceId);
		Task<PagedQueryResult<ApiResource>> PagedQueryAsync(string keyword, int page, int limit);
		Task<bool> DisableAsync(Guid apiResourceId);

		Task<bool> EnableAsync(Guid apiResourceId);
		Task<string> GetSecretsAsync(Guid apiResourceId);
		Task<bool> UpdateSecretsAsync(Guid apiResourceId, string secrets);
		Task<ApiScope> GetScopeAsync(Guid id);
	}
}
