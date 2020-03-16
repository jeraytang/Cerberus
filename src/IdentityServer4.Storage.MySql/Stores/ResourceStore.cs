using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using IdentityServer4.Models;
using IdentityServer4.Storage.Adapter;
using IdentityServer4.Storage.Adapter.Entities;
using IdentityServer4.Stores;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using ApiResource = IdentityServer4.Models.ApiResource;
using IdentityResource = IdentityServer4.Models.IdentityResource;

namespace IdentityServer4.Storage.MySql.Stores
{
	public class ResourceStore : IResourceStore
	{
		private readonly IdentityServerOptions _options;
		private readonly ILogger<ResourceStore> _logger;
		private readonly IMapper _mapper;

		private const string ApiResourceAndScopeColumns = @"
       a.Enabled  as ApiResourceEnabled,
       a.Name                    as ApiResourceName,
       a.DisplayName             as ApiResourceDisplayName,
       a.Description             as ApiResourceDescription,
       a.UserClaims              as ApiResourceUserClaims,
       a.ApiSecrets                 as ApiResourceSecrets,
       a.Properties              as ApiResourceProperties,
       b.Name                    as ScopeName,
       b.DisplayName             as ScopeDisplayName,
       b.Description             as ScopeDescription,
       b.Required                as ScopeRequired,
       b.Emphasize               as ScopeEmphasize,
       b.ShowInDiscoveryDocument as ScopeShowInDiscoveryDocument,
       b.UserClaims              as ScopeUserClaims";

		public ResourceStore(IdentityServerOptions options, ILogger<ResourceStore> logger, IMapper mapper)
		{
			_options = options;
			_logger = logger;
			_mapper = mapper;
		}

		public async Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeAsync(
			IEnumerable<string> scopeNames)
		{
			var scopes = scopeNames.ToArray();
			await using var conn = new MySqlConnection(_options.ConnectionString);

			var resources = await conn.QueryAsync<IdentityServer4.Storage.Adapter.Entities.IdentityResource>(
				$"SELECT * FROM IdentityResources WHERE `Name` IN @Names", new {Names = scopes});

			var results = resources.ToArray();

			_logger.LogDebug("Found {scopes} identity scopes in database", results.Select(x => x.Name));

			return results.Select(x => _mapper.Map<IdentityResource>(x)).ToArray();
		}

		public async Task<IEnumerable<ApiResource>> FindApiResourcesByScopeAsync(IEnumerable<string> scopeNames)
		{
			var scopes = scopeNames.ToArray();
			await using var conn = new MySqlConnection(_options.ConnectionString);

			var data = (await conn.QueryAsync<ApiResourceAndScope>(
					$@"SELECT
{ApiResourceAndScopeColumns}
FROM ApiResources a JOIN ApiScopes b ON a.Id = b.ApiResourceId WHERE b.Name IN @Names", new {Names = scopes}))
				.ToList();
			var apis = ToApiResources(data);
			return apis;
		}

		public async Task<ApiResource> FindApiResourceAsync(string name)
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);

			var data = (await conn.QueryAsync<ApiResourceAndScope>(
				$@"SELECT
{ApiResourceAndScopeColumns}
FROM ApiResources a JOIN ApiScopes b ON a.Id = b.ApiResourceId WHERE a.Name = @Name", new {Name = name})).ToList();

			if (data.Count == 0)
			{
				_logger.LogDebug("Did not find {api} API resource in database", name);
				return null;
			}

			var apiResource = ToApiResource(data);
			_logger.LogDebug("Found {api} API resource in database", name);
			return apiResource;
		}

		public async Task<Resources> GetAllResourcesAsync()
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);

			var identity = await conn.QueryAsync<IdentityServer4.Storage.Adapter.Entities.IdentityResource>(
				$"SELECT * FROM IdentityResources");

			var data = (await conn.QueryAsync<ApiResourceAndScope>(
				$@"SELECT
{ApiResourceAndScopeColumns}
FROM ApiResources a JOIN ApiScopes b ON a.Id = b.ApiResourceId")).ToList();

			var apis = ToApiResources(data);
			var result = new Resources(identity.Select(x => _mapper.Map<IdentityResource>(x)), apis);

			_logger.LogDebug("Found {scopes} as all scopes in database",
				result.IdentityResources.Select(x => x.Name)
					.Union(result.ApiResources.SelectMany(x => x.Scopes).Select(x => x.Name)));
			return result;
		}

		private IEnumerable<ApiResource> ToApiResources(IEnumerable<ApiResourceAndScope> data)
		{
			var dict = new Dictionary<string, List<ApiResourceAndScope>>();
			foreach (var entity in data)
			{
				if (!dict.ContainsKey(entity.ApiResourceName))
				{
					dict.Add(entity.ApiResourceName, new List<ApiResourceAndScope>());
				}

				dict[entity.ApiResourceName].Add(entity);
			}

			return dict.Select(x => ToApiResource(x.Value));
		}

		private ApiResource ToApiResource(List<ApiResourceAndScope> data)
		{
			var apiResource = new IdentityServer4.Storage.Adapter.Entities.ApiResource
			{
				Description = data[0].ApiResourceDescription,
				Enabled = data[0].ApiResourceEnabled,
				Name = data[0].ApiResourceName,
				Properties = data[0].ApiResourceProperties,
				ApiSecrets = data[0].ApiResourceSecrets,
				DisplayName = data[0].ApiResourceDisplayName,
				UserClaims = data[0].ApiResourceUserClaims
			};

			var scopes = data.Select(entity => new ApiScope
				{
					Emphasize = entity.ScopeEmphasize,
					Description = entity.ScopeDescription,
					DisplayName = entity.ScopeDisplayName,
					Name = entity.ScopeName,
					Required = entity.ScopeRequired,
					ShowInDiscoveryDocument = entity.ScopeShowInDiscoveryDocument,
					UserClaims = entity.ScopeUserClaims
				})
				.ToList();

			var result = _mapper.Map<ApiResource>(apiResource);
			result.Scopes = scopes.Select(x => _mapper.Map<Scope>(x)).ToHashSet();
			return result;
		}
	}

	public class ApiResourceAndScope
	{
		/// <summary>
		/// Indicates if this resource is enabled. Defaults to true.
		/// </summary>
		public bool ApiResourceEnabled { get; set; } = true;

		/// <summary>
		/// The unique name of the resource.
		/// </summary>
		public string ApiResourceName { get; set; }

		/// <summary>
		/// Display name of the resource.
		/// </summary>
		public string ApiResourceDisplayName { get; set; }

		/// <summary>
		/// Description of the resource.
		/// </summary>
		public string ApiResourceDescription { get; set; }

		public string ApiResourceUserClaims { get; set; }

		public string ApiResourceSecrets { get; set; }

		public string ApiResourceProperties { get; set; }

		/// <summary>
		/// Name of the scope. This is the value a client will use to request the scope.
		/// </summary>
		public string ScopeName { get; set; }

		/// <summary>
		/// Display name. This value will be used e.g. on the consent screen.
		/// </summary>
		public string ScopeDisplayName { get; set; }

		/// <summary>
		/// Description. This value will be used e.g. on the consent screen.
		/// </summary>
		public string ScopeDescription { get; set; }

		/// <summary>
		/// Specifies whether the user can de-select the scope on the consent screen. Defaults to false.
		/// </summary>
		public bool ScopeRequired { get; set; } = false;

		/// <summary>
		/// Specifies whether the consent screen will emphasize this scope. Use this setting for sensitive or important scopes. Defaults to false.
		/// </summary>
		public bool ScopeEmphasize { get; set; } = false;

		/// <summary>
		/// Specifies whether this scope is shown in the discovery document. Defaults to true.
		/// </summary>
		public bool ScopeShowInDiscoveryDocument { get; set; } = true;

		public string ScopeUserClaims { get; set; }
	}
}
