using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using IdentityServer4.Admin.Common;
using IdentityServer4.Admin.DTO;
using IdentityServer4.Storage.Adapter.Entities;
using IdentityServer4.Storage.Adapter.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer4.Admin.Controllers.API
{
	[ApiController]
	[Route("api/v1.0/identityResources")]
	[Authorize(Roles = "admin, cerberus-admin")]
	public class IdentityResourceController : ControllerBase
	{
		private readonly IIdentityResourceRepository _identityResourceRepository;
		private readonly IMapper _mapper;

		public IdentityResourceController(IIdentityResourceRepository identityResourceRepository, IMapper mapper)
		{
			_identityResourceRepository = identityResourceRepository;
			_mapper = mapper;
		}

		[HttpPost]
		public async Task<bool> AddAsync(AddIdentityResourceDTO dto)
		{
			var entity = _mapper.Map<IdentityResource>(dto);
			entity.SetCreationAudited(HttpContext.GetUserId(), HttpContext.GetUserName());
			return (await _identityResourceRepository.AddAsync(entity)) == 1;
		}

		[HttpGet("{id}")]
		public async Task<IdentityResourceDTO> GetAsync(Guid id)
		{
			var entity = await _identityResourceRepository.GetAsync(id);
			return _mapper.Map<IdentityResourceDTO>(entity);
		}

		[HttpGet]
		public async Task<Storage.Adapter.PagedQueryResult<ListIdentityResourceDTO>> PagedQueryAsync(int page,
			int limit)
		{
			var result = await _identityResourceRepository.PagedQueryAsync(page, limit);
			return new Storage.Adapter.PagedQueryResult<ListIdentityResourceDTO>
			{
				Count = result.Count,
				Limit = result.Limit,
				Page = result.Page,
				Entities = _mapper.Map<List<ListIdentityResourceDTO>>(result.Entities)
			};
		}

		[HttpPut("{id}")]
		public async Task<bool> UpdateAsync(Guid id, IdentityResourceDTO dto)
		{
			dto.Id = id;
			var origin = await _identityResourceRepository.GetAsync(id);
			if (origin == null)
			{
				return false;
			}

			var entity = _mapper.Map(dto, origin);
			entity.SetModificationAudited(HttpContext.GetUserId(), HttpContext.GetUserName());
			return await _identityResourceRepository.UpdateAsync(entity);
		}

		[HttpPut("{identityResourceId}/disable")]
		public async Task<bool> DisableAsync(Guid identityResourceId)
		{
			return await _identityResourceRepository.DisableAsync(identityResourceId);
		}

		[HttpPut("{identityResourceId}/enable")]
		public async Task<bool> EnableAsync(Guid identityResourceId)
		{
			return await _identityResourceRepository.EnableAsync(identityResourceId);
		}

		[HttpDelete("{identityResourceId}")]
		public async Task<bool> DeleteAsync(Guid identityResourceId)
		{
			return await _identityResourceRepository.DeleteAsync(identityResourceId);
		}
	}
}
