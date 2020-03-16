using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using IdentityServer4.Models;
using IdentityServer4.Storage.Adapter;
using IdentityServer4.Stores;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace IdentityServer4.Storage.MySql.Stores
{
	public class PersistedGrantStore : IPersistedGrantStore
	{
		private readonly ILogger _logger;
		private readonly IdentityServerOptions _options;

		public PersistedGrantStore(IdentityServerOptions options, ILogger<PersistedGrantStore> logger)
		{
			_options = options;
			_logger = logger;
		}

		public async Task StoreAsync(PersistedGrant token)
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			var sql =
				$@"INSERT IGNORE INTO PersistedGrants (`Key`, `Type`, `SubjectId`, `ClientId`, `CreationTime`, `Expiration`, `Data`)
VALUES (@Key, @Type, @SubjectId, @ClientId, @CreationTime, @Expiration, @Data)
ON DUPLICATE key UPDATE `Key` = @Key, `Type` = @Type, `SubjectId` = @SubjectId, `ClientId` = @ClientId, `CreationTime` = @CreationTime, `Expiration` = @Expiration, `Data` = @Data";

			await conn.ExecuteScalarAsync(sql, token);
		}

		public async Task<PersistedGrant> GetAsync(string key)
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			var sql = $"SELECT * FROM PersistedGrants WHERE `Key` = @Key";

			var result = await conn.QuerySingleOrDefaultAsync<PersistedGrant>(sql, new {Key = key});
			_logger.LogDebug("{persistedGrantKey} found in database: {persistedGrantKeyFound}", key, result != null);
			return result;
		}

		public async Task<IEnumerable<PersistedGrant>> GetAllAsync(string subjectId)
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			return await conn.QueryAsync<PersistedGrant>(
				$"SELECT * FROM PersistedGrants WHERE `SubjectId` = @SubjectId", new {SubjectId = subjectId});
		}

		public async Task RemoveAsync(string key)
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			var sql = $"DELETE FROM PersistedGrants WHERE `Key` = @Key";
			await conn.ExecuteAsync(sql, new {Key = key});
		}

		public async Task RemoveAllAsync(string subjectId, string clientId)
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			var sql = $"DELETE FROM PersistedGrants WHERE `SubjectId` = @SubjectId AND `ClientId` = @ClientId";
			await conn.ExecuteAsync(sql, new {SubjectId = subjectId, ClientId = clientId});
		}

		public async Task RemoveAllAsync(string subjectId, string clientId, string type)
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			var sql =
				$"DELETE FROM PersistedGrants WHERE `SubjectId` = @SubjectId AND `ClientId` = @ClientId AND `Type` = @Type";
			await conn.ExecuteAsync(sql, new {SubjectId = subjectId, ClientId = clientId, Type = type});
		}
	}
}
