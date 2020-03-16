using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cerberus.API.Data;
using Cerberus.API.DTO;
using Cerberus.API.Extensions;
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
    [Route("api/v1.0/services")]
    [Authorize(Roles = "cerberus-admin,admin")]
    public class ServiceController : MSFrameworkApiControllerBase
    {
        private readonly IdentityDbContext _dbContext;
        private readonly IMapper _mapper;

        public ServiceController(
            IMapper mapper,
            IdentityDbContext dbContext,
            IMSFrameworkSession session,
            ILogger<ServiceController> logger) : base(session, logger)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        #region MSFramework

        [AllowAnonymous]
        [HttpHead("{serviceId}")]
        public async Task<IActionResult> Exists(string serviceId)
        {
            serviceId.NotNullOrWhiteSpace(nameof(serviceId));

            var entity = await _dbContext.Services.FirstOrDefaultAsync(x => x.Id == serviceId);
            if (entity == null)
            {
                return NotFound();
            }

            if (!HttpContext.IsAuth(entity.SecurityToken))
            {
                throw new ApplicationException("Access dined");
            }

            return Ok();
        }

        [AllowAnonymous]
        [HttpGet("{serviceId}/permissions")]
        public async Task<IEnumerable<Permission>> GetPermissions(string serviceId)
        {
            serviceId.NotNullOrWhiteSpace(nameof(serviceId));
            var entity = await _dbContext.Services.FirstOrDefaultAsync(x => x.Id == serviceId);
            if (entity == null)
            {
                throw new ApplicationException("Service exists");
            }

            if (!HttpContext.IsAuth(entity.SecurityToken))
            {
                throw new ApplicationException("Access dined");
            }

            return _dbContext.Permissions.Where(x => x.ServiceId == serviceId);
        }

        [AllowAnonymous]
        [HttpPost("{serviceId}/permissions")]
        public async Task<bool> CreateAsync(string serviceId, [FromBody] CreateServicePermissionDTO dto)
        {
            serviceId.NotNullOrWhiteSpace(nameof(serviceId));

            var entity = await _dbContext.Services.FirstOrDefaultAsync(x => x.Id == serviceId);

            if (entity == null)
            {
                throw new ApplicationException($"Service not exists");
            }

            if (!HttpContext.IsAuth(entity.SecurityToken))
            {
                throw new ApplicationException("Access dined");
            }

            var permission = new Permission(serviceId, dto.Type, dto.Module, dto.Name, dto.Identification,
                dto.Description);
            permission.SetCreationAudited("System", "System");
            permission.SetModificationAudited("System", "System", DateTimeOffset.Now);

            await _dbContext.Permissions.AddAsync(permission);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        [AllowAnonymous]
        [HttpPut("{serviceId}/permissions/{ids}/expire")]
        public async Task<bool> ExpireAsync(string serviceId, string ids)
        {
            serviceId.NotNullOrWhiteSpace(nameof(serviceId));
            ids.NotNullOrWhiteSpace(nameof(ids));

            var entity = await _dbContext.Services.FirstOrDefaultAsync(x => x.Id == serviceId);

            if (entity == null)
            {
                throw new ApplicationException($"Service not exists");
            }

            if (!HttpContext.IsAuth(entity.SecurityToken))
            {
                throw new ApplicationException("Access dined");
            }

            var array = ids.Split(',');
            if (array.Length > 0)
            {
                var permissions = await _dbContext.Permissions.Where(x => ids.Contains(x.Id)).ToListAsync();
                if (permissions.Any(x => x.ServiceId != serviceId))
                {
                    throw new ApplicationException($"Access dined");
                }

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

        [AllowAnonymous]
        [HttpPut("{serviceId}/permissions/{ids}/renewal")]
        public async Task<bool> RenewalAsync(string serviceId, string ids)
        {
            serviceId.NotNullOrWhiteSpace(nameof(serviceId));
            ids.NotNullOrWhiteSpace(nameof(ids));

            var entity = await _dbContext.Services.FirstOrDefaultAsync(x => x.Id == serviceId);

            if (entity == null)
            {
                throw new ApplicationException($"Service not exists");
            }

            if (!HttpContext.IsAuth(entity.SecurityToken))
            {
                throw new ApplicationException("Access dined");
            }

            var array = ids.Split(',');
            if (array.Length > 0)
            {
                var permissions = await _dbContext.Permissions.Where(x => ids.Contains(x.Id)).ToListAsync();
                if (permissions.Any(x => x.ServiceId != serviceId))
                {
                    throw new ApplicationException($"Access dined");
                }

                permissions.ForEach(x =>
                {
                    x.Renewal();
                    x.SetCreationAudited(Session.UserId, Session.UserName);
                    x.SetModificationAudited(Session.UserId, Session.UserName, DateTimeOffset.Now);
                    _dbContext.Update(x);
                });
                await _dbContext.SaveChangesAsync();
            }

            return true;
        }

        #endregion

        #region admin

        [HttpGet]
        public async Task<PagedQueryResult<ListServiceDTO>> QueryAsync(string keyword,
            int page,
            int limit)
        {
            PagedQueryResult<Service> result;
            if (string.IsNullOrWhiteSpace(keyword))
            {
                result = await _dbContext.Services.PagedQueryAsync(page, limit,
                    null,
                    new OrderCondition<Service, DateTimeOffset?>(x => x.LastModificationTime, true));
            }
            else
            {
                result = await _dbContext.Services.PagedQueryAsync(page, limit,
                    x => x.Name.Contains(keyword.Trim()),
                    new OrderCondition<Service, DateTimeOffset?>(x => x.LastModificationTime, true));
            }

            return _mapper.ToPagedQueryResultDTO<ListServiceDTO>(result);
        }

        [HttpPost]
        public async Task<bool> CreateAsync([FromBody] CreateServiceDTO dto)
        {
            if ((await _dbContext.Services.FirstOrDefaultAsync(x => x.Name == dto.Name.Trim())) != null)
            {
                throw new ApplicationException("Service exists");
            }

            var entity = new Service(dto.Name, dto.SecurityToken, dto.Description);
            entity.SetCreationAudited(Session.UserId, Session.UserName);
            entity.SetModificationAudited(Session.UserId, Session.UserName, DateTimeOffset.Now);

            await _dbContext.Services.AddAsync(entity);

            await _dbContext.SaveChangesAsync();
            return true;
        }

        [HttpDelete("{id}")]
        public async Task<bool> DeleteAsync(string id)
        {
            var service = await _dbContext.Services.FirstOrDefaultAsync(x => x.Id == id);
            if (service == null)
            {
                throw new ArgumentException("Service not exists");
            }

            _dbContext.Services.Remove(service);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        [HttpGet("{id}")]
        public async Task<ServiceDTO> GetAsync(string id)
        {
            var entity = await _dbContext.Services.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
            {
                throw new ArgumentException("Service not exists");
            }

            return _mapper.Map<ServiceDTO>(entity);
        }

        [HttpPut("{id}")]
        public async Task<bool> UpdateAsync(string id, [FromBody] ServiceDTO dto)
        {
            var entity = await _dbContext.Services.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
            {
                throw new ArgumentException("Service not exists");
            }

            entity.Modify(dto.Name, dto.SecurityToken, dto.Description);
            entity.SetModificationAudited(Session.UserId, Session.UserName);

            _dbContext.Services.Update(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        #endregion
    }
}