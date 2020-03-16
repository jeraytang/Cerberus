using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using IdentityServer4.Storage.Adapter;
using IdentityServer4.Storage.Adapter.Entities;
using IdentityServer4.Stores;
using MySql.Data.MySqlClient;

namespace IdentityServer4.Storage.MySql.Stores
{
	public class ClientStore : IClientStore
	{
		private readonly IdentityServerOptions _options;
		private readonly IMapper _mapper;

		public ClientStore(IdentityServerOptions options, IMapper mapper)
		{
			_options = options;
			_mapper = mapper;
		}

		public async Task<IdentityServer4.Models.Client> FindClientByIdAsync(string clientId)
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			var client = await conn.QuerySingleOrDefaultAsync<Client>(
				$"SELECT * FROM Clients  WHERE ClientId = @ClientId",
				new {ClientId = clientId});
			if (client == null)
			{
				return null;
			}

			var bo = _mapper.Map<IdentityServer4.Models.Client>(client);
			return bo;
		}
	}
}
