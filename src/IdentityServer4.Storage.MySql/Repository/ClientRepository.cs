using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using IdentityServer4.Storage.Adapter;
using IdentityServer4.Storage.Adapter.Entities;
using IdentityServer4.Storage.Adapter.Extensions;
using IdentityServer4.Storage.Adapter.Repository;
using MySql.Data.MySqlClient;

namespace IdentityServer4.Storage.MySql.Repository
{
	public class ClientRepository : IClientRepository
	{
		private readonly IdentityServerOptions _options;

		public static string InsertClientSql =
			@"INSERT INTO Clients (Id,Enabled,ClientId,ProtocolType,ClientSecrets,RequireClientSecret,ClientName,Description,ClientUri,LogoUri,
RequireConsent,AllowRememberConsent,AllowedGrantTypes,RequirePkce,AllowPlainTextPkce,AllowAccessTokensViaBrowser,RedirectUris,PostLogoutRedirectUris,FrontChannelLogoutUri,
FrontChannelLogoutSessionRequired,BackChannelLogoutUri,BackChannelLogoutSessionRequired,AllowOfflineAccess,AllowedScopes,AlwaysIncludeUserClaimsInIdToken,IdentityTokenLifetime,
AccessTokenLifetime,AuthorizationCodeLifetime,AbsoluteRefreshTokenLifetime,SlidingRefreshTokenLifetime,ConsentLifetime,RefreshTokenUsage,UpdateAccessTokenClaimsOnRefresh,
RefreshTokenExpiration,AccessTokenType,EnableLocalLogin,IdentityProviderRestrictions,IncludeJwtId,Claims,AlwaysSendClientClaims,ClientClaimsPrefix,PairWiseSubjectSalt,
UserSsoLifetime,UserCodeType,DeviceCodeLifetime,AllowedCorsOrigins,Properties,CreationUserId,CreationUserName,CreationTime)
VALUES (@Id,@Enabled,@ClientId,@ProtocolType,@ClientSecrets,@RequireClientSecret,@ClientName,@Description,@ClientUri,@LogoUri,
@RequireConsent,@AllowRememberConsent,@AllowedGrantTypes,@RequirePkce,@AllowPlainTextPkce,@AllowAccessTokensViaBrowser,@RedirectUris,@PostLogoutRedirectUris,@FrontChannelLogoutUri,
@FrontChannelLogoutSessionRequired,@BackChannelLogoutUri,@BackChannelLogoutSessionRequired,@AllowOfflineAccess,@AllowedScopes,@AlwaysIncludeUserClaimsInIdToken,@IdentityTokenLifetime,
@AccessTokenLifetime,@AuthorizationCodeLifetime,@AbsoluteRefreshTokenLifetime,@SlidingRefreshTokenLifetime,@ConsentLifetime,@RefreshTokenUsage,@UpdateAccessTokenClaimsOnRefresh,
@RefreshTokenExpiration,@AccessTokenType,@EnableLocalLogin,@IdentityProviderRestrictions,@IncludeJwtId,@Claims,@AlwaysSendClientClaims,@ClientClaimsPrefix,@PairWiseSubjectSalt,
@UserSsoLifetime,@UserCodeType,@DeviceCodeLifetime,@AllowedCorsOrigins,@Properties,@CreationUserId,@CreationUserName,CURRENT_TIMESTAMP());";

		public static string InsertClientCorsOriginsSql =
			"INSERT INTO ClientCorsOrigins (Id,Origin,ClientId) VALUES (@Id,@Origin,@ClientId);";

		public static string DisableClientSql = "UPDATE Clients SET Enabled = false WHERE Id = @Id";

		public static string EnableClientSql = "UPDATE Clients SET Enabled = true WHERE Id = @Id";

		public static string UpdateClientSql =
			@"UPDATE Clients SET Enabled=@Enabled,ClientId=@ClientId,ProtocolType=@ProtocolType,ClientSecrets=@ClientSecrets,RequireClientSecret=@RequireClientSecret,
ClientName=@ClientName,Description=@Description,ClientUri=@ClientUri,LogoUri=@LogoUri,RequireConsent=@RequireConsent,AllowRememberConsent=@AllowRememberConsent,
AllowedGrantTypes=@AllowedGrantTypes,RequirePkce=@RequirePkce,AllowPlainTextPkce=@AllowPlainTextPkce,AllowAccessTokensViaBrowser=@AllowAccessTokensViaBrowser,
RedirectUris=@RedirectUris,PostLogoutRedirectUris=@PostLogoutRedirectUris,FrontChannelLogoutUri=@FrontChannelLogoutUri,FrontChannelLogoutSessionRequired=@FrontChannelLogoutSessionRequired,
BackChannelLogoutUri=@BackChannelLogoutUri,BackChannelLogoutSessionRequired=@BackChannelLogoutSessionRequired,AllowOfflineAccess=@AllowOfflineAccess,AllowedScopes=@AllowedScopes,
AlwaysIncludeUserClaimsInIdToken=@AlwaysIncludeUserClaimsInIdToken,IdentityTokenLifetime=@IdentityTokenLifetime,AccessTokenLifetime=@AccessTokenLifetime,
AuthorizationCodeLifetime=@AuthorizationCodeLifetime,AbsoluteRefreshTokenLifetime=@AbsoluteRefreshTokenLifetime,SlidingRefreshTokenLifetime=@SlidingRefreshTokenLifetime,
ConsentLifetime=@ConsentLifetime,RefreshTokenUsage=@RefreshTokenUsage,UpdateAccessTokenClaimsOnRefresh=@UpdateAccessTokenClaimsOnRefresh,
RefreshTokenExpiration=@RefreshTokenExpiration,AccessTokenType=@AccessTokenType,EnableLocalLogin=@EnableLocalLogin,IdentityProviderRestrictions=@IdentityProviderRestrictions,
IncludeJwtId=@IncludeJwtId,Claims=@Claims,AlwaysSendClientClaims=@AlwaysSendClientClaims,ClientClaimsPrefix=@ClientClaimsPrefix,PairWiseSubjectSalt=@PairWiseSubjectSalt,
UserSsoLifetime=@UserSsoLifetime,UserCodeType=@UserCodeType,DeviceCodeLifetime=@DeviceCodeLifetime,AllowedCorsOrigins=@AllowedCorsOrigins,Properties=@Properties,
LastModificationUserId=@LastModificationUserId,LastModificationUserName=@LastModificationUserName,LastModificationTime=CURRENT_TIMESTAMP() WHERE Id = @Id";

		public static string DeleteClientSql = "DELETE FROM Clients WHERE Id = @Id";

		public ClientRepository(IdentityServerOptions options)
		{
			_options = options;
		}

		public async Task<int> CountAsync()
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			return conn.QuerySingleOrDefault<int>($"SELECT COUNT(*) FROM Clients");
		}

		public async Task<int> AddAsync(params Client[] clients)
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);

			if (conn.State != ConnectionState.Open)
			{
				await conn.OpenAsync();
			}

			var transaction = await conn.BeginTransactionAsync();
			try
			{
				var effectedRows = await conn.ExecuteAsync(InsertClientSql
					, clients,
					transaction);
				var clientCorsOrigins = new List<ClientCorsOrigins>();
				foreach (var client in clients)
				{
					var allowedCorsOrigins = client.AllowedCorsOrigins.ToHashsetBySeparator('\n');
					clientCorsOrigins.AddRange(allowedCorsOrigins.Select(origin => new ClientCorsOrigins
					{
						ClientId = client.Id, Origin = origin
					}));
				}

				await conn.ExecuteAsync(InsertClientCorsOriginsSql,
					clientCorsOrigins, transaction);
				await transaction.CommitAsync();
				return effectedRows;
			}
			catch
			{
				await transaction.RollbackAsync();
				throw;
			}
		}

		public int Add(params Client[] clients)
		{
			using var conn = new MySqlConnection(_options.ConnectionString);

			if (conn.State != ConnectionState.Open)
			{
				conn.Open();
			}

			var transaction = conn.BeginTransaction();
			try
			{
				var effectedRows = conn.Execute(InsertClientSql, clients, transaction);
				var clientCorsOrigins = new List<ClientCorsOrigins>();
				foreach (var client in clients)
				{
					var allowedCorsOrigins = client.AllowedCorsOrigins.ToHashsetBySeparator('\n');
					clientCorsOrigins.AddRange(allowedCorsOrigins.Select(origin => new ClientCorsOrigins
					{
						ClientId = client.Id, Origin = origin
					}));
				}

				conn.Execute(InsertClientCorsOriginsSql, clientCorsOrigins, transaction);
				transaction.Commit();
				return effectedRows;
			}
			catch
			{
				transaction.Rollback();
				throw;
			}
		}

		public async Task<bool> UpdateAsync(Client client)
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);

			if (conn.State != ConnectionState.Open)
			{
				await conn.OpenAsync();
			}

			var transaction = await conn.BeginTransactionAsync();
			try
			{
				var effectedRows = await conn.ExecuteAsync(UpdateClientSql, client, transaction);
				// todo: 优化成删除，添加
				await conn.ExecuteAsync($"DELETE FROM ClientCorsOrigins WHERE ClientId=@ClientId",
					new {ClientId = client.Id}, transaction);
				if (!string.IsNullOrWhiteSpace(client.AllowedCorsOrigins))
				{
					var allowedCorsOrigins = client.AllowedCorsOrigins.ToHashsetBySeparator('\n');
					await conn.ExecuteAsync(
						$"INSERT INTO ClientCorsOrigins (Id,Origin,ClientId) VALUES (@Id,@Origin,@ClientId);",
						allowedCorsOrigins.Select(x => new ClientCorsOrigins {ClientId = client.Id, Origin = x}),
						transaction);
				}

				await transaction.CommitAsync();
				return effectedRows == 1;
			}
			catch
			{
				await transaction.RollbackAsync();
				throw;
			}
		}

		public async Task<bool> DeleteAsync(Guid id)
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			return (await conn.ExecuteAsync(DeleteClientSql, new {Id = id})) == 1;
		}

		public async Task<PagedQueryResult<Client>> PagedQueryAsync(string keyword, int page,
			int limit)
		{
			page = page <= 0 ? 1 : page;

			limit = limit > 50 ? 50 : limit;
			limit = limit <= 0 ? 1 : limit;

			await using var conn = new MySqlConnection(_options.ConnectionString);
			var whereSql = string.IsNullOrWhiteSpace(keyword)
				? ""
				: $"WHERE ClientId LIKE CONCAT('{keyword.Trim()}','%')";
			var totalSql =
				$"SELECT COUNT(*) FROM Clients {whereSql}";
			var total = await conn.QuerySingleAsync<int>(totalSql);
			var clients = await conn.QueryAsync<Client>(
				$"SELECT * FROM Clients {whereSql} ORDER BY CreationTime DESC LIMIT @Start,@Limit",
				new {Start = (page - 1) * limit, Limit = limit});
			var result = new PagedQueryResult<Client>
			{
				Page = page, Count = total, Limit = limit, Entities = clients.ToList()
			};
			return result;
		}

		public async Task<List<string>> GetAllowedCorsOriginsAsync()
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			var allowedCorsOrigins =
				(await conn.QueryAsync<string>($"SELECT Origin FROM ClientCorsOrigins GROUP BY Origin"))
				.ToList();
			return allowedCorsOrigins;
		}

		public async Task<bool> DisableAsync(Guid id)
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			return (await conn.ExecuteAsync(DisableClientSql, new {Id = id})) == 1;
		}

		public async Task<bool> EnableAsync(Guid id)
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			return (await conn.ExecuteAsync(EnableClientSql, new {Id = id})) == 1;
		}

		public async Task<Client> GetAsync(Guid id)
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			return await conn.QuerySingleOrDefaultAsync<Client>(
				$"SELECT * FROM Clients  WHERE Id = @Id",
				new {Id = id});
		}

		public async Task<string> GetSecretsAsync(Guid id)
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			return await conn.QuerySingleOrDefaultAsync<string>(
				$"SELECT ClientSecrets FROM Clients  WHERE Id = @Id",
				new {Id = id});
		}

		public async Task<bool> UpdateSecretsAsync(Guid id, string secrets)
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			return (await conn.ExecuteAsync("UPDATE Clients SET ClientSecrets = @ClientSecrets WHERE Id = @Id",
				       new {Id = id, ClientSecrets = secrets})) == 1;
		}
	}
}
