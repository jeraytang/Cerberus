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
    [Route("api/v1.0/apiResources")]
    [Authorize(Roles = "admin, cerberus-admin")]
    public class ApiResourceController : ControllerBase
    {
        private readonly IApiResourceRepository _apiResourceRepository;
        private readonly IMapper _mapper;

        public ApiResourceController(IApiResourceRepository apiResourceRepository, IMapper mapper)
        {
            _apiResourceRepository = apiResourceRepository;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        public async Task<ApiResourceDTO> GetAsync(Guid id)
        {
            var entity = await _apiResourceRepository.GetAsync(id);
            entity.Name = entity.Name.Trim();
            return _mapper.Map<ApiResourceDTO>(entity);
        }

        [HttpPost]
        public async Task<bool> AddAsync(AddApiResourceDTO dto)
        {
            var entity = _mapper.Map<ApiResource>(dto);
            entity.Name = entity.Name.Trim();
            entity.SetCreationAudited(HttpContext.GetUserId(), HttpContext.GetUserName());
            return await _apiResourceRepository.AddAsync(entity) == 1;
        }

        [HttpGet("{resourceId}/scopes")]
        public async Task<List<ListApiResourceScopeDTO>> GetScopesAsync(Guid resourceId)
        {
            var scopes = await _apiResourceRepository.GetScopesAsync(resourceId);
            return _mapper.Map<List<ListApiResourceScopeDTO>>(scopes);
        }

        [HttpPost("{resourceId}/scopes")]
        public async Task<bool> AddScopeAsync(Guid resourceId, AddApiResourceScopeDTO dto)
        {
            var entity = _mapper.Map<ApiScope>(dto);
            entity.SetCreationAudited(HttpContext.GetUserId(), HttpContext.GetUserName());
            entity.ApiResourceId = resourceId;
            entity.Name = entity.Name.Trim();
            return (await _apiResourceRepository.AddScopeAsync(entity)) == 1;
        }

        [HttpPut("{resourceId}/scopes/{id}")]
        public async Task<bool> UpdateScopeAsync(Guid id, ApiResourceScopeDTO dto)
        {
            var scope = await _apiResourceRepository.GetScopeAsync(id);
            if (scope == null)
            {
                return false;
            }

            var entity = _mapper.Map(dto, scope);
            entity.Name = entity.Name.Trim();
            entity.SetModificationAudited(HttpContext.GetUserId(), HttpContext.GetUserName());
            return await _apiResourceRepository.UpdateScopeAsync(entity);
        }

        [HttpDelete("{resourceId}/scopes/{id}")]
        public async Task<IApiResult> DeleteScopeAsync(Guid resourceId, Guid id)
        {
            // var resource = await _apiResourceRepository.GetAsync(resourceId);
            // if (resource == null)
            // {
            // 	return new ApiResult(false);
            // }
            //
            // var scope = await _apiResourceRepository.GetScopeAsync(id);
            // if (scope == null)
            // {
            // 	return new ApiResult(false);
            // }
            //
            // if (scope.Name == resource.Name)
            // {
            // 	return new ErrorApiResult("Should not remove the api name scope");
            // }

            var result = await _apiResourceRepository.DeleteScopeAsync(resourceId, id);
            return new ApiResult(result);
        }

        [HttpGet("{resourceId}/scopes/{id}")]
        public async Task<ApiResourceScopeDTO> GetScopeAsync(Guid id)
        {
            var scope = await _apiResourceRepository.GetScopeAsync(id);
            return _mapper.Map<ApiResourceScopeDTO>(scope);
        }

        [HttpGet]
        public async Task<Storage.Adapter.PagedQueryResult<ListApiResourceDTO>> PagedQueryAsync(string keyword,
            int page,
            int limit)
        {
            var result = await _apiResourceRepository.PagedQueryAsync(keyword, page, limit);
            return new Storage.Adapter.PagedQueryResult<ListApiResourceDTO>
            {
                Count = result.Count,
                Limit = result.Limit,
                Page = result.Page,
                Entities = _mapper.Map<List<ListApiResourceDTO>>(result.Entities)
            };
        }

        [HttpPut("{id}")]
        public async Task<bool> UpdateAsync(Guid id, ApiResourceDTO dto)
        {
            dto.Id = id;
            var origin = await _apiResourceRepository.GetAsync(id);
            if (origin == null)
            {
                return false;
            }

            var entity = _mapper.Map(dto, origin);
            entity.Name = entity.Name.Trim();
            return await _apiResourceRepository.UpdateAsync(entity);
        }

        [HttpPut("{apiResourceId}/disable")]
        public async Task<bool> DisableAsync(Guid apiResourceId)
        {
            return await _apiResourceRepository.DisableAsync(apiResourceId);
        }

        [HttpPut("{apiResourceId}/enable")]
        public async Task<bool> EnableAsync(Guid apiResourceId)
        {
            return await _apiResourceRepository.EnableAsync(apiResourceId);
        }

        [HttpDelete("{apiResourceId}")]
        public async Task<bool> DeleteAsync(Guid apiResourceId)
        {
            return await _apiResourceRepository.DeleteAsync(apiResourceId);
        }

        [HttpGet("{clientId}/secrets")]
        public async Task<IEnumerable<ListSecretDTO>> SecretsAsync(Guid clientId)
        {
            var secrets = await _apiResourceRepository.GetSecretsAsync(clientId);
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

            var secrets = await _apiResourceRepository.GetSecretsAsync(clientId);
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
            bool success = await _apiResourceRepository.UpdateSecretsAsync(clientId, secrets);
            return new ApiResult(true, success);
        }

        [HttpDelete("{clientId}/secrets/{secretId}")]
        public async Task<bool> DeleteSecretsAsync(Guid clientId, Guid secretId)
        {
            var secrets = await _apiResourceRepository.GetSecretsAsync(clientId);
            var secretList = string.IsNullOrWhiteSpace(secrets)
                ? new List<Secret>()
                : JsonConvert.DeserializeObject<List<Secret>>(secrets);
            var secret = secretList.FirstOrDefault(x => x.Id == secretId);
            if (secret != null)
            {
                secretList.Remove(secret);
                secrets = JsonConvert.SerializeObject(secretList);
                return await _apiResourceRepository.UpdateSecretsAsync(clientId, secrets);
            }
            else
            {
                return false;
            }
        }
    }
}