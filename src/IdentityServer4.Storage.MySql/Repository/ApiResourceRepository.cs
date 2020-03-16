using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using IdentityServer4.Storage.Adapter;
using IdentityServer4.Storage.Adapter.Entities;
using IdentityServer4.Storage.Adapter.Repository;
using MySql.Data.MySqlClient;

namespace IdentityServer4.Storage.MySql.Repository
{
	public class ApiResourceRepository : IApiResourceRepository
	{
		private readonly IdentityServerOptions _options;
		private readonly IMapper _mapper;

		public static string InsertScopeSql =
			$@"INSERT INTO ApiScopes (Id, Emphasize,Name,ApiResourceId,Description,DisplayName,Required,ShowInDiscoveryDocument,UserClaims,CreationUserId,CreationUserName,CreationTime)
VALUES (@Id,@Emphasize,@Name,@ApiResourceId,@Description,@DisplayName,@Required,@ShowInDiscoveryDocument,@UserClaims,@CreationUserId,@CreationUserName,CURRENT_TIMESTAMP());";

		public static string DisableIdentityResourceSql = $"UPDATE ApiResources SET Enabled = false WHERE Id = @Id";
		public static string EnableIdentityResourceSql = $"UPDATE ApiResources SET Enabled = true WHERE Id = @Id";

		public ApiResourceRepository(IdentityServerOptions options, IMapper mapper)
		{
			_options = options;
			_mapper = mapper;
		}

		public async Task<int> AddAsync(params ApiResource[] apiResources)
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			if (conn.State != ConnectionState.Open)
			{
				await conn.OpenAsync();
			}

			var transaction = await conn.BeginTransactionAsync();
			try
			{
				var effectedRows = await conn.ExecuteAsync(
					$@"INSERT INTO ApiResources (Id,Enabled,Name,DisplayName,Description,UserClaims,ApiSecrets,Properties,CreationUserId,CreationUserName,CreationTime)
 VALUES (@Id,@Enabled,@Name,@DisplayName,@Description,@UserClaims,@ApiSecrets,@Properties,@CreationUserId,@CreationUserName,CURRENT_TIMESTAMP());",
					apiResources, transaction);
				var scopes = apiResources.Select(x =>
				{
					var scope = new ApiScope
					{
						ApiResourceId = x.Id,
						Name = x.Name,
						DisplayName = x.DisplayName,
						Description = x.Description
					};
					scope.SetCreationAudited(x.CreationUserId, x.CreationUserName, x.CreationTime);
					return scope;
				});
				await conn.ExecuteAsync(InsertScopeSql, scopes, transaction);
				await transaction.CommitAsync();
				return effectedRows;
			}
			catch
			{
				await transaction.RollbackAsync();
				throw;
			}
		}

		public async Task<int> AddScopeAsync(params ApiScope[] scopes)
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			if (conn.State != ConnectionState.Open)
			{
				await conn.OpenAsync();
			}

			var transaction = await conn.BeginTransactionAsync();
			try
			{
				var effectedRows = await conn.ExecuteAsync(InsertScopeSql, scopes, transaction);
				await transaction.CommitAsync();
				return effectedRows;
			}
			catch
			{
				await transaction.RollbackAsync();
				throw;
			}
		}

		public async Task<bool> UpdateScopeAsync(ApiScope scope)
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			return (await conn.ExecuteAsync(
				       $@"UPDATE ApiScopes SET Emphasize=@Emphasize,Name=@Name,Description=@Description,DisplayName=@DisplayName,
Required=@Required,ShowInDiscoveryDocument=@ShowInDiscoveryDocument,UserClaims=@UserClaims,
LastModificationUserId=@LastModificationUserId,LastModificationUserName=@LastModificationUserName,LastModificationTime=CURRENT_TIMESTAMP() WHERE Id=@Id AND ApiResourceId=@ApiResourceId;",
				       scope)) == 1;
		}

		public async Task<bool> DeleteScopeAsync(Guid resourceId, Guid scopeId)
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			return (await conn.ExecuteAsync(
				       $@"DELETE FROM ApiScopes WHERE Id=@Id AND ApiResourceId=@ApiResourceId;",
				       new {ApiResourceId = resourceId, Id = scopeId})) == 1;
		}

		public async Task<bool> UpdateAsync(ApiResource apiResource)
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			return (await conn.ExecuteAsync(
				       $@"UPDATE ApiResources SET Enabled=@Enabled,Name=@Name,DisplayName=@DisplayName,Description=@Description,UserClaims=@UserClaims,
ApiSecrets=@ApiSecrets,Properties=@Properties,LastModificationUserId=@LastModificationUserId,LastModificationUserName=@LastModificationUserName,LastModificationTime=CURRENT_TIMESTAMP() WHERE Id=@Id;",
				       apiResource)) == 1;
		}

		public async Task<bool> DeleteAsync(Guid apiResourceId)
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			return (await conn.ExecuteAsync(
				       $@"DELETE FROM ApiResources WHERE Id=@Id;", new {Id = apiResourceId})) == 1;
		}

		public async Task<PagedQueryResult<ApiResource>> PagedQueryAsync(string keyword, int page, int limit)
		{
			page = page <= 0 ? 1 : page;

			limit = limit > 50 ? 50 : limit;
			limit = limit <= 0 ? 1 : limit;

			await using var conn = new MySqlConnection(_options.ConnectionString);
			var whereSql = string.IsNullOrWhiteSpace(keyword) ? "" : $"WHERE Name LIKE CONCAT('{keyword.Trim()}','%')";
			var totalSql =
				$"SELECT COUNT(*) FROM ApiResources {whereSql}";
			var total = await conn.QuerySingleAsync<int>(totalSql);
			var apiResources = await conn.QueryAsync<ApiResource>(
				$"SELECT * FROM ApiResources {whereSql} LIMIT @Start,@Limit",
				new {Start = (page - 1) * limit, Limit = limit});
			var result = new PagedQueryResult<ApiResource>
			{
				Page = page,
				Count = total,
				Limit = limit,
				Entities = apiResources.Select(x => _mapper.Map<ApiResource>(x)).ToList()
			};
			return result;
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

		public async Task<string> GetSecretsAsync(Guid id)
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			return await conn.QuerySingleOrDefaultAsync<string>(
				$"SELECT ApiSecrets FROM ApiResources  WHERE Id = @Id",
				new {Id = id});
		}

		public async Task<bool> UpdateSecretsAsync(Guid id, string secrets)
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			return (await conn.ExecuteAsync("UPDATE ApiResources SET ApiSecrets = @ApiSecrets WHERE Id = @Id",
				       new {Id = id, ApiSecrets = secrets})) == 1;
		}

		public async Task<ApiScope> GetScopeAsync(Guid id)
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			var scope = await conn.QuerySingleOrDefaultAsync<ApiScope>($"SELECT * FROM ApiScopes WHERE Id=@Id",
				new {Id = id});
			return scope;
		}

		public async Task<List<ApiScope>> GetScopesAsync(Guid apiResourceId)
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			var scopes = (await conn.QueryAsync<ApiScope>($"SELECT * FROM ApiScopes WHERE ApiResourceId=@ApiResourceId",
				new {ApiResourceId = apiResourceId})).ToList();
			return scopes;
		}

		public async Task<ApiResource> GetAsync(Guid id)
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			return await conn.QuerySingleOrDefaultAsync<ApiResource>(
				$"SELECT * FROM ApiResources  WHERE Id = @Id",
				new {Id = id});
		}
	}
}
