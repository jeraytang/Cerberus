using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cerberus.API.Data;
using Cerberus.API.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MSFramework.AspNetCore;
using MSFramework.Data;
using MSFramework.Domain;
using MSFramework.Extensions;

namespace Cerberus.API.Controllers
{
	[ApiController]
	[Route("api/v1.0/roles")]
	[Authorize(Roles = "cerberus-admin,admin")]
	public class RoleController : MSFrameworkApiControllerBase
	{
		private readonly IdentityDbContext _dbContext;
		private readonly IMapper _mapper;

		public RoleController(IMapper mapper,
			IdentityDbContext dbContext, IMSFrameworkSession session,
			ILogger<RoleController> logger) : base(session, logger)
		{
			_dbContext = dbContext;
			_mapper = mapper;
		}

		[HttpGet("{id}/permissions")]
		public async Task<RolePermissionsDTO> GetPermissionsAsync(string id)
		{
			var entity = await _dbContext.Roles.FirstOrDefaultAsync(x => x.Id == id);
			if (entity == null)
			{
				throw new ArgumentException("Role not exists");
			}

			var result = new RolePermissionsDTO {Role = entity.Name, Services = new List<ServicePermissionsDTO>()};

			var permissions = await _dbContext.Permissions.Include(x => x.Service).ToListAsync();

			var dict = await _dbContext.RolePermissions.Where(x => x.RoleId == id)
				.ToDictionaryAsync(x => x.PermissionId, x => x);

			var permissionsGroupByService = permissions.GroupBy(x => x.Service.Name);

			foreach (var service in permissionsGroupByService)
			{
				var servicePermissionsDto =
					new ServicePermissionsDTO {Service = service.Key, Groups = new List<GroupPermissionDTO>()};
				var servicePermissions = service.ToList();
				var groups = servicePermissions.GroupBy(x => x.Module);

				foreach (var group in groups)
				{
					var groupPermission = new GroupPermissionDTO
					{
						Permissions = new List<GrantPermissionDTO>(), Module = group.Key
					};

					foreach (var permission in group)
					{
						groupPermission.Permissions.Add(new GrantPermissionDTO
						{
							Name = permission.Name,
							Id = permission.Id,
							HasPermission = dict.ContainsKey(permission.Id)
						});
					}

					servicePermissionsDto.Groups.Add(groupPermission);
				}

				result.Services.Add(servicePermissionsDto);
			}

			return result;
		}

		[HttpPost("{id}/permissions/{permissionId}")]
		public async Task<bool> AddPermission(string id, string permissionId)
		{
			id.NotNullOrWhiteSpace(nameof(id));
			permissionId.NotNullOrWhiteSpace(nameof(permissionId));
			var entity = await _dbContext.Roles.FirstOrDefaultAsync(x => x.Id == id);
			if (entity == null)
			{
				throw new ArgumentException("Role not exists");
			}

			var permission = await _dbContext.Permissions.FirstOrDefaultAsync(x => x.Id == permissionId);
			if (permission == null)
			{
				throw new ArgumentException("Permission not exists");
			}

			var rolePermission = new RolePermission(id, permissionId);
			await _dbContext.RolePermissions.AddAsync(rolePermission);
			await _dbContext.SaveChangesAsync();
			return true;
		}

		[HttpDelete("{id}/permissions/{permissionId}")]
		public async Task<bool> DeletePermission(string id, string permissionId)
		{
			id.NotNullOrWhiteSpace(nameof(id));
			permissionId.NotNullOrWhiteSpace(nameof(permissionId));
			var entity =
				await _dbContext.RolePermissions.FirstOrDefaultAsync(x =>
					x.RoleId == id && x.PermissionId == permissionId);
			if (entity == null)
			{
				throw new ArgumentException("Role not exists");
			}

			_dbContext.RolePermissions.Remove(entity);
			await _dbContext.SaveChangesAsync();
			return true;
		}

		[HttpGet]
		public async Task<PagedQueryResult<ListRoleDTO>> QueryAsync(string keyword,
			int page,
			int limit)
		{
			PagedQueryResult<Role> result;
			if (string.IsNullOrWhiteSpace(keyword))
			{
				result = await _dbContext.Roles.PagedQueryAsync(page, limit,
					null,
					new OrderCondition<Role, DateTimeOffset?>(x => x.LastModificationTime, true));
			}
			else
			{
				result = await _dbContext.Roles.PagedQueryAsync(page, limit,
					x => x.Name.Contains(keyword.Trim()),
					new OrderCondition<Role, DateTimeOffset?>(x => x.LastModificationTime, true));
			}

			return _mapper.ToPagedQueryResultDTO<ListRoleDTO>(result);
		}

		[HttpPost]
		public async Task<bool> CreateAsync([FromBody] CreateRoleDTO dto)
		{
			var entity = new Role(dto.Name, dto.Type, dto.Description);
			entity.SetCreationAudited(Session.UserId, Session.UserName);
			entity.SetModificationAudited(null, null, DateTimeOffset.Now);

			await _dbContext.Roles.AddAsync(entity);

			await _dbContext.SaveChangesAsync();
			return true;
		}

		[HttpDelete("{id}")]
		public async Task<bool> DeleteAsync(string id)
		{
			var entity = await _dbContext.Roles.FirstOrDefaultAsync(x => x.Id == id);
			if (entity == null)
			{
				throw new ArgumentException("Role not exists");
			}

			if (entity.Name == "admin" || entity.Name == "cerberus-admin" || entity.Name == "tenant-admin")
			{
				throw new ApplicationException("Access dined");
			}

			_dbContext.Remove(entity);
			await _dbContext.SaveChangesAsync();
			return true;
		}

		[HttpGet("{id}")]
		public async Task<RoleDTO> GetAsync(string id)
		{
			var entity = await _dbContext.Roles.FirstOrDefaultAsync(x => x.Id == id);
			if (entity == null)
			{
				throw new ArgumentException("Role not exists");
			}

			return _mapper.Map<RoleDTO>(entity);
		}

		[HttpPut("{id}")]
		public async Task<bool> UpdateAsync(string id, [FromBody] RoleDTO dto)
		{
			var entity = await _dbContext.Roles.FirstOrDefaultAsync(x => x.Id == id);
			if (entity == null)
			{
				throw new ArgumentException("Role not exists");
			}

			entity.SetModificationAudited(Session.UserId, Session.UserName);
			entity.Modify(dto.Name, dto.Type, dto.Description);

			_dbContext.Roles.Update(entity);
			await _dbContext.SaveChangesAsync();
			return true;
		}
	}
}
