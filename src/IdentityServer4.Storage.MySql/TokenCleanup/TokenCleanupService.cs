using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using IdentityServer4.Models;
using IdentityServer4.Storage.Adapter;
using IdentityServer4.Storage.Adapter.Entities;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace IdentityServer4.Storage.MySql.TokenCleanup
{
	public class TokenCleanupService
	{
		private readonly ILogger<TokenCleanupService> _logger;
		private readonly IOperationalStoreNotification _operationalStoreNotification;
		private readonly IdentityServerOptions _options;

		public TokenCleanupService(
			IdentityServerOptions options,
			ILogger<TokenCleanupService> logger,
			IOperationalStoreNotification operationalStoreNotification = null)
		{
			_options = options ?? throw new ArgumentNullException(nameof(options));
			if (_options.TokenCleanupBatchSize < 1)
			{
				throw new ArgumentException("Token cleanup batch size interval must be at least 1");
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_operationalStoreNotification = operationalStoreNotification;
		}

		/// <summary>
		/// Method to clear expired persisted grants.
		/// </summary>
		/// <returns></returns>
		public async Task RemoveExpiredGrantsAsync()
		{
			try
			{
				_logger.LogTrace("Querying for expired grants to remove");

				await RemoveGrantsAsync();
				await RemoveDeviceCodesAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError("Exception removing expired grants: {exception}", ex.Message);
			}
		}

		/// <summary>
		/// Removes the stale persisted grants.
		/// </summary>
		/// <returns></returns>
		protected virtual async Task RemoveGrantsAsync()
		{
			var found = Int32.MaxValue;
			await using var conn = new MySqlConnection(_options.ConnectionString);
			while (found >= _options.TokenCleanupBatchSize)
			{
				var expiredGrants = (await conn.QueryAsync<PersistedGrant>(
					$"SELECT * FROM PersistedGrants WHERE `Expiration` < @Expiration ORDER BY `Key` LIMIT @LIMIT",
					new {Expiration = DateTime.UtcNow, Limit = _options.TokenCleanupBatchSize})).ToList();


				found = expiredGrants.Count;
				_logger.LogInformation("Removing {grantCount} grants", found);

				if (found > 0)
				{
					await conn.ExecuteAsync($"DELETE FROM PersistedGrants WHERE `Key` IN @Keys;",
						new {Keys = expiredGrants.Select(x => x.Key).ToArray()});

					if (_operationalStoreNotification != null)
					{
						await _operationalStoreNotification.PersistedGrantsRemovedAsync(expiredGrants);
					}
				}
			}
		}

		/// <summary>
		/// Removes the stale device codes.
		/// </summary>
		/// <returns></returns>
		protected virtual async Task RemoveDeviceCodesAsync()
		{
			var found = Int32.MaxValue;
			await using var conn = new MySqlConnection(_options.ConnectionString);
			while (found >= _options.TokenCleanupBatchSize)
			{
				var expiredCodes = (await conn.QueryAsync<DeviceFlowCodes>(
					$"SELECT * FROM DeviceCodes WHERE `Expiration` < @Expiration ORDER BY `DeviceCode` LIMIT @LIMIT",
					new {Expiration = DateTime.UtcNow, Limit = _options.TokenCleanupBatchSize})).ToList();

				found = expiredCodes.Count;
				_logger.LogInformation("Removing {deviceCodeCount} device flow codes", found);

				if (found > 0)
				{
					await conn.ExecuteAsync($"DELETE FROM DeviceCodes WHERE `DeviceCode` IN @DeviceCodes;",
						new {DeviceCodes = expiredCodes.Select(x => x.DeviceCode).ToArray()});
				}
			}
		}
	}
}
