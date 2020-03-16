using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Storage.Adapter.Entities;

namespace IdentityServer4.Storage.Adapter.Repository
{
	public interface IClientRepository
	{
		Task<int> CountAsync();
		Task<int> AddAsync(params Client[] client);
		int Add(params Client[] client);

		Task<bool> UpdateAsync(Client client);
		Task<bool> DeleteAsync(Guid id);
		Task<PagedQueryResult<Client>> PagedQueryAsync(string keyword, int page, int limit);
		Task<List<string>> GetAllowedCorsOriginsAsync();
		Task<bool> DisableAsync(Guid id);
		Task<bool> EnableAsync(Guid id);
		Task<Client> GetAsync(Guid id);
		Task<string> GetSecretsAsync(Guid id);
		Task<bool> UpdateSecretsAsync(Guid id, string secrets);
	}
}
