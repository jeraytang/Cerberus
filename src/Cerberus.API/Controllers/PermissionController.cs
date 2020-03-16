using System;
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
	[Route("api/v1.0/permissions")]
	[Authorize(Roles = "cerberus-admin,admin")]
	public class PermissionController : MSFrameworkApiControllerBase
	{
		private readonly IdentityDbContext _dbContext;
		private readonly IMapper _mapper;

		public PermissionController(IMapper mapper,
			IdentityDbContext dbContext,
			IMSFrameworkSession session,
			ILogger<PermissionController> logger) : base(session, logger)
		{
			_dbContext = dbContext;
			_mapper = mapper;
		}

		[HttpPut("{ids}/expire")]
		public async Task<bool> ExpireAsync(string ids)
		{
			ids.NotNullOrWhiteSpace(nameof(ids));

			var array = ids.Split(',');
			if (array.Length > 0)
			{
				var permissions = await _dbContext.Permissions.Where(x => ids.Contains(x.Id)).ToListAsync();
				permissions.ForEach(x =>
				{
					x.Expire();
					x.SetCreationAudited(Session.UserId, Session.UserName);
					x.SetModificationAudited(Session.UserId, Session.UserName, DateTimeOffset.Now);
					_dbContext.Update(x);
				});
				await _dbContext.SaveChangesAsync();
			}

			return true;
		}

		[HttpPut("{ids}/renewal")]
		public async Task<bool> RenewalAsync(string ids)
		{
			ids.NotNullOrWhiteSpace(nameof(ids));

			var array = ids.Split(',');
			if (array.Length > 0)
			{
				var permissions = await _dbContext.Permissions.Where(x => ids.Contains(x.Id)).ToListAsync();
				permissions.ForEach(x =>
				{
					x.Renewal();
					x.SetModificationAudited(Session.UserId, Session.UserName, DateTimeOffset.Now);
					_dbContext.Update(x);
				});
				await _dbContext.SaveChangesAsync();
			}

			return true;
		}

		[HttpGet]
		public async Task<PagedQueryResult<ListPermissionDTO>> QueryAsync(string keyword, string serviceId,
			string module,
			int page,
			int limit)
		{
			PagedQueryResult<Permission> result;
			if (!string.IsNullOrWhiteSpace(serviceId))
			{
				if (string.IsNullOrWhiteSpace(keyword))
				{
					if (string.IsNullOrWhiteSpace(module))
					{
						// keyword null, serviceId not null, module null
						result = await _dbContext.Permissions.Include(x => x.Service).PagedQueryAsync(page, limit,
							x => x.ServiceId == serviceId,
							new OrderCondition<Permission, DateTimeOffset?>(x => x.LastModificationTime, true));
					}
					else
					{
						// keyword null, serviceId not null, module not null
						result = await _dbContext.Permissions.Include(x => x.Service).PagedQueryAsync(page, limit,
							x => x.ServiceId == serviceId && x.Module == module.Trim(),
							new OrderCondition<Permission, DateTimeOffset?>(x => x.LastModificationTime, true));
					}
				}
				else
				{
					if (string.IsNullOrWhiteSpace(module))
					{
						// keyword not null, serviceId not null, module null
						result = await _dbContext.Permissions.Include(x => x.Service).PagedQueryAsync(page, limit,
							x => x.Name.Contains(keyword.Trim()) &&
							     x.ServiceId == serviceId,
							new OrderCondition<Permission, DateTimeOffset?>(x => x.LastModificationTime, true));
					}
					else
					{
						// keyword not null, serviceId not null, module not null
						result = await _dbContext.Permissions.Include(x => x.Service).PagedQueryAsync(page, limit,
							x => x.Name.Contains(keyword.Trim()) &&
							     x.ServiceId == serviceId &&
							     x.Module == module.Trim(),
							new OrderCondition<Permission, DateTimeOffset?>(x => x.LastModificationTime, true));
					}
				}
			}
			else
			{
				if (string.IsNullOrWhiteSpace(keyword))
				{
					if (string.IsNullOrWhiteSpace(module))
					{
						// keyword null, serviceId null, module null
						result = await _dbContext.Permissions.Include(x => x.Service).PagedQueryAsync(page, limit,
							null,
							new OrderCondition<Permission, DateTimeOffset?>(x => x.LastModificationTime, true));
					}
					else
					{
						// keyword null, serviceId null, module not null
						result = await _dbContext.Permissions.Include(x => x.Service).PagedQueryAsync(page, limit,
							x => x.Module == module.Trim(),
							new OrderCondition<Permission, DateTimeOffset?>(x => x.LastModificationTime, true));
					}
				}
				else
				{
					if (string.IsNullOrWhiteSpace(module))
					{
						// keyword not null, serviceId null, module null
						result = await _dbContext.Permissions.Include(x => x.Service).PagedQueryAsync(page, limit,
							x => x.Name.Contains(keyword.Trim()),
							new OrderCondition<Permission, DateTimeOffset?>(x => x.LastModificationTime, true));
					}
					else
					{
						// keyword not null, serviceId null, module not null
						result = await _dbContext.Permissions.Include(x => x.Service).PagedQueryAsync(page, limit,
							x => x.Name.Contains(keyword.Trim()) && x.Module == module.Trim(),
							new OrderCondition<Permission, DateTimeOffset?>(x => x.LastModificationTime, true));
					}
				}
			}

			return _mapper.ToPagedQueryResultDTO<ListPermissionDTO>(result);
		}

		[HttpPost]
		public async Task<bool> CreateAsync([FromBody] CreatePermissionDTO dto)
		{
			var service = await _dbContext.Services.FirstOrDefaultAsync(x => x.Id == dto.ServiceId.Trim());

			if (service == null)
			{
				throw new ApplicationException($"Service not exists");
			}

			var entity = new Permission(dto.ServiceId, dto.Type, dto.Module, dto.Name, dto.Identification,
				dto.Description);

			entity.SetCreationAudited(Session.UserId, Session.UserName);
			entity.SetModificationAudited(Session.UserId, Session.UserName, DateTimeOffset.Now);

			await _dbContext.Permissions.AddAsync(entity);
			await _dbContext.SaveChangesAsync();
			return true;
		}

		[HttpDelete("{id}")]
		public async Task<bool> DeleteAsync(string id)
		{
			var entity = await _dbContext.Permissions.FirstOrDefaultAsync(x => x.Id == id);
			if (entity == null)
			{
				throw new ArgumentException("Permission not exists");
			}

			_dbContext.Remove(entity);
			await _dbContext.SaveChangesAsync();
			return true;
		}

		[HttpGet("{id}")]
		public async Task<PermissionDTO> GetAsync(string id)
		{
			var entity = await _dbContext.Permissions.FirstOrDefaultAsync(x => x.Id == id);
			if (entity == null)
			{
				throw new ArgumentException("Permission not exists");
			}

			return _mapper.Map<PermissionDTO>(entity);
		}

		[HttpPut("{id}")]
		public async Task<bool> UpdateAsync(string id, [FromBody] PermissionDTO dto)
		{
			var service = await _dbContext.Services.FirstOrDefaultAsync(x => x.Id == dto.ServiceId.Trim());

			if (service == null)
			{
				throw new ApplicationException($"Service not exists");
			}

			var entity = await _dbContext.Permissions.FirstOrDefaultAsync(x => x.Id == id);
			if (entity == null)
			{
				throw new ArgumentException("Permission not exists");
			}

			var type = Enum.Parse<PermissionType>(dto.Type);
			entity.Modify(dto.ServiceId, type, dto.Module, dto.Name, dto.Identification, dto.Description);
			entity.SetModificationAudited(Session.UserId, Session.UserName);

			_dbContext.Permissions.Update(entity);
			await _dbContext.SaveChangesAsync();
			return true;
		}
	}
}
