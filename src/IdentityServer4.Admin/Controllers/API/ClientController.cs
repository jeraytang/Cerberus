using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using IdentityModel;
using IdentityServer4.Admin.Common;
using IdentityServer4.Admin.DTO;
using IdentityServer4.Storage.Adapter.Entities;
using IdentityServer4.Storage.Adapter.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IdentityServer4.Admin.Controllers.API
{
	[ApiController]
	[Route("api/v1.0/clients")]
	[Authorize(Roles = "admin, cerberus-admin")]
	public class ClientController : ControllerBase
	{
		private readonly IClientRepository _clientRepository;
		private readonly IMapper _mapper;

		public ClientController(IClientRepository clientRepository, IMapper mapper)
		{
			_clientRepository = clientRepository;
			_mapper = mapper;
		}

		[HttpPost]
		public async Task<bool> AddAsync(AddClientDTO dto)
		{
			var client = _mapper.Map<Client>(dto);
			client.SetCreationAudited(HttpContext.GetUserId(), HttpContext.GetUserName());
			return (await _clientRepository.AddAsync(client)) == 1;
		}

		[HttpGet("{id}")]
		public async Task<ClientDTO> GetAsync(Guid id)
		{
			var client = await _clientRepository.GetAsync(id);
			return _mapper.Map<ClientDTO>(client);
		}

		[HttpPut("{id}")]
		public async Task<bool> UpdateAsync(Guid id, ClientDTO dto)
		{
			dto.Id = id;
			var origin = await _clientRepository.GetAsync(id);
			if (origin == null)
			{
				return false;
			}

			var entity = _mapper.Map(dto, origin);
			entity.SetModificationAudited(HttpContext.GetUserId(), HttpContext.GetUserName());
			return await _clientRepository.UpdateAsync(entity);
		}

		[HttpGet]
		public async Task<Storage.Adapter.PagedQueryResult<ListClientDTO>> PagedQueryAsync(string keyword, int page,
			int limit)
		{
			var result = await _clientRepository.PagedQueryAsync(keyword, page, limit);
			return new Storage.Adapter.PagedQueryResult<ListClientDTO>
			{
				Count = result.Count,
				Limit = result.Limit,
				Page = result.Page,
				Entities = _mapper.Map<List<ListClientDTO>>(result.Entities)
			};
		}

		[HttpPut("{clientId}/disable")]
		public async Task<bool> DisableAsync(Guid clientId)
		{
			return await _clientRepository.DisableAsync(clientId);
		}

		[HttpPut("{clientId}/enable")]
		public async Task<bool> EnableAsync(Guid clientId)
		{
			return await _clientRepository.EnableAsync(clientId);
		}

		[HttpDelete("{clientId}")]
		public async Task<bool> DeleteAsync(Guid clientId)
		{
			return await _clientRepository.DeleteAsync(clientId);
		}

		[HttpGet("{clientId}/secrets")]
		public async Task<IEnumerable<ListSecretDTO>> SecretsAsync(Guid clientId)
		{
			var secrets = await _clientRepository.GetSecretsAsync(clientId);
			return string.IsNullOrWhiteSpace(secrets)
				? new ListSecretDTO[0]
				: JsonConvert.DeserializeObject<ListSecretDTO[]>(secrets);
		}

		[HttpPost("{clientId}/secrets")]
		public async Task<IApiResult> AddSecretsAsync(Guid clientId, SecretDTO dto)
		{
			DateTime? expiration = null;
			if (!string.IsNullOrWhiteSpace(dto.Expiration))
			{
				expiration = DateTime.Parse(dto.Expiration);
				if (expiration.Value < DateTime.Now)
				{
					return new ErrorApiResult("Expiration should larger than now");
				}
			}

			var secrets = await _clientRepository.GetSecretsAsync(clientId);
			var secretList = string.IsNullOrWhiteSpace(secrets)
				? new HashSet<Secret>()
				: JsonConvert.DeserializeObject<HashSet<Secret>>(secrets);

			if (secretList.Count == 5)
			{
				return new ErrorApiResult("Count of secrets should less than 5");
			}

			var secret = new Secret
			{
				Value = dto.Value.ToSha256(),
				Type = dto.Type,
				Description = dto.Description,
				Expiration = expiration
			};

			secretList.Add(secret);
			secrets = JsonConvert.SerializeObject(secretList);
			var success = await _clientRepository.UpdateSecretsAsync(clientId, secrets);
			return new ApiResult(true, success);
		}

		[HttpDelete("{clientId}/secrets/{secretId}")]
		public async Task<bool> DeleteSecretsAsync(Guid clientId, Guid secretId)
		{
			var secrets = await _clientRepository.GetSecretsAsync(clientId);
			var secretList = string.IsNullOrWhiteSpace(secrets)
				? new List<Secret>()
				: JsonConvert.DeserializeObject<List<Secret>>(secrets);
			var secret = secretList.FirstOrDefault(x => x.Id == secretId);
			if (secret != null)
			{
				secretList.Remove(secret);
				secrets = JsonConvert.SerializeObject(secretList);
				return await _clientRepository.UpdateSecretsAsync(clientId, secrets);
			}
			else
			{
				return false;
			}
		}
	}
}
