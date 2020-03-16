using System;
using System.Threading.Tasks;
using Dapper;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Storage.Adapter;
using IdentityServer4.Storage.Adapter.Entities;
using IdentityServer4.Stores;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace IdentityServer4.Storage.MySql.Stores
{
	public class DeviceFlowStore : IDeviceFlowStore
	{
		private readonly IdentityServerOptions _options;
		private readonly ILogger<DeviceFlowStore> _logger;

		public DeviceFlowStore(IdentityServerOptions options, ILogger<DeviceFlowStore> logger)
		{
			_options = options;
			_logger = logger;
		}

		public async Task StoreDeviceAuthorizationAsync(string deviceCode, string userCode, DeviceCode data)
		{
			var po = ToDeviceFlowCodes(data, deviceCode, userCode);
			await using var conn = new MySqlConnection(_options.ConnectionString);
			await conn.ExecuteAsync(
				$@"INSERT INTO DeviceCodes (UserCode,DeviceCode,SubjectId,ClientId,CreationTime,Expiration,Data)
 VALUES (@UserCode,@DeviceCode,@SubjectId,@ClientId,@CreationTime,@Expiration,@Data)", po);
		}

		public async Task<DeviceCode> FindByUserCodeAsync(string userCode)
		{
			var deviceFlowCodes = await FindDeviceFlowCodesByUserCodeAsync(userCode);
			var model = ToDeviceCode(deviceFlowCodes?.Data);

			_logger.LogDebug("{userCode} found in database: {userCodeFound}", userCode, model != null);

			return model;
		}

		public async Task<DeviceCode> FindByDeviceCodeAsync(string deviceCode)
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			var deviceFlowCodes = await conn.QuerySingleOrDefaultAsync<DeviceFlowCodes>(
				$"SELECT * FROM DeviceCodes WHERE DeviceCode=@DeviceCode", new {DeviceCode = deviceCode});

			var model = ToDeviceCode(deviceFlowCodes?.Data);

			_logger.LogDebug("{deviceCode} found in database: {deviceCodeFound}", deviceCode, model != null);
			return model;
		}

		public async Task UpdateByUserCodeAsync(string userCode, DeviceCode data)
		{
			var deviceFlowCodes = await FindDeviceFlowCodesByUserCodeAsync(userCode);
			if (deviceFlowCodes == null)
			{
				_logger.LogError("{userCode} not found in database", userCode);
				throw new InvalidOperationException("Could not update device code");
			}

			var entity = ToDeviceFlowCodes(data, deviceFlowCodes.DeviceCode, userCode);
			_logger.LogDebug("{userCode} found in database", userCode);

			entity.SubjectId = data.Subject?.FindFirst(JwtClaimTypes.Subject).Value;
			entity.Data = entity.Data;
			await using var conn = new MySqlConnection(_options.ConnectionString);
			await conn.ExecuteAsync(
				$@"UPDATE DeviceCodes SET UserCode=@UserCode,SubjectId=@SubjectId,ClientId=@ClientId,CreationTime=@CreationTime,Expiration=@Expiration,@Data WHERE DeviceCode=@DeviceCode",
				entity);
		}

		public async Task RemoveByDeviceCodeAsync(string deviceCode)
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			await conn.ExecuteAsync(
				$"DELETE FROM DeviceCodes WHERE DeviceCode=@DeviceCode", new {DeviceCode = deviceCode});
		}

		protected DeviceFlowCodes ToDeviceFlowCodes(DeviceCode model, string deviceCode, string userCode)
		{
			if (model == null || deviceCode == null || userCode == null) return null;

			return new DeviceFlowCodes
			{
				DeviceCode = deviceCode,
				UserCode = userCode,
				ClientId = model.ClientId,
				SubjectId = model.Subject?.FindFirst(JwtClaimTypes.Subject).Value,
				CreationTime = model.CreationTime,
				Expiration = model.CreationTime.AddSeconds(model.Lifetime),
				Data = JsonConvert.SerializeObject(model)
			};
		}

		/// <summary>
		/// Converts a serialized DeviceCode to a model.
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		protected DeviceCode ToDeviceCode(string entity)
		{
			if (entity == null) return null;
			return JsonConvert.DeserializeObject<DeviceCode>(entity);
		}

		protected async Task<DeviceFlowCodes> FindDeviceFlowCodesByUserCodeAsync(string userCode)
		{
			await using var conn = new MySqlConnection(_options.ConnectionString);
			return await conn.QuerySingleOrDefaultAsync<DeviceFlowCodes>(
				$"SELECT * FROM DeviceCodes WHERE UserCode=@UserCode", new {UserCode = userCode});
		}
	}
}
