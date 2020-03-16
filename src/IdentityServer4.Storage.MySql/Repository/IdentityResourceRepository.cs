using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using IdentityServer4.Storage.Adapter;
using IdentityServer4.Storage.Adapter.Entities;
using IdentityServer4.Storage.Adapter.Repository;
using MySql.Data.MySqlClient;

namespace IdentityServer4.Storage.MySql.Repository
{
	public class IdentityResourceRepository : IIdentityResourceRepository
	{
		private readonly IdentityServerOptions _options;

		public static string DisableIdentityResourceSql = "UPDATE IdentityResources SET Enabled = false WHERE Id = @Id";

		public static string EnableIdentityResourceSql = "UPDATE IdentityResources SET Enabled = true WHERE Id = @Id";

		public IdentityResourceRepository(IdentityServerOptions options)
		{
			_options = options;
		}

		public async Task<int> AddAsync(params IdentityResource[] identityResources)
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			return await conn.ExecuteAsync(
				$@"INSERT INTO IdentityResources (Id,Required,Emphasize,ShowInDiscoveryDocument,Enabled,Name,DisplayName,Description,UserClaims,Properties,
CreationUserId,CreationUserName,CreationTime)
VALUES (@Id,@Required,@Emphasize,@ShowInDiscoveryDocument,@Enabled,@Name,@DisplayName,@Description,@UserClaims,@Properties,@CreationUserId,@CreationUserName,CURRENT_TIMESTAMP());",
				identityResources);
		}

		public async Task<bool> UpdateAsync(IdentityResource identityResource)
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			return (await conn.ExecuteAsync(
				       $@"UPDATE IdentityResources SET Required=@Required,Emphasize=@Emphasize,ShowInDiscoveryDocument=@ShowInDiscoveryDocument,Enabled=@Enabled,
Name=@Name,DisplayName=@DisplayName,Description=@Description,UserClaims=@UserClaims,Properties=@Properties,
LastModificationUserId=@LastModificationUserId,LastModificationUserName=@LastModificationUserName,LastModificationTime=CURRENT_TIMESTAMP() WHERE Id=@Id;",
				       identityResource)) == 1;
		}

		public async Task<bool> DeleteAsync(Guid identityResourceId)
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			return (await conn.ExecuteAsync(
				       $@"DELETE FROM IdentityResources WHERE Id=@Id;", new {Id = identityResourceId})) == 1;
		}

		public async Task<bool> ExistsAsync(string identity)
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			var id = await conn.QuerySingleOrDefaultAsync<Guid>(
				$@"SELECT ID FROM IdentityResources WHERE Name=@Name LIMIT 1;", new {Name = identity});
			return id != default;
		}

		public async Task<PagedQueryResult<IdentityResource>> PagedQueryAsync(int page,
			int limit)
		{
			page = page <= 0 ? 1 : page;

			limit = limit > 50 ? 50 : limit;
			limit = limit <= 0 ? 1 : limit;

			await using var conn = new MySqlConnection(_options.ConnectionString);
			var total = await conn.QuerySingleAsync<int>($"SELECT COUNT(*) FROM IdentityResources");
			var identityResources = await conn.QueryAsync<IdentityResource>(
				$"SELECT * FROM IdentityResources LIMIT @Start,@Limit",
				new {Start = (page - 1) * limit, Limit = limit});
			var result = new PagedQueryResult<IdentityResource>
			{
				Page = page, Count = total, Limit = limit, Entities = identityResources.ToList()
			};
			return result;
		}

		public async Task<IdentityResource> GetAsync(Guid id)
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			return await conn.QuerySingleOrDefaultAsync<IdentityResource>(
				$"SELECT * FROM IdentityResources  WHERE Id = @Id",
				new {Id = id});
		}

		public async Task<bool> DisableAsync(Guid id)
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			return (await conn.ExecuteAsync(DisableIdentityResourceSql, new {Id = id})) == 1;
		}

		public async Task<bool> EnableAsync(Guid id)
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			return (await conn.ExecuteAsync(EnableIdentityResourceSql, new {Id = id})) == 1;
		}
	}
}
