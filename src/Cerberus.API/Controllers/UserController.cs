using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cerberus.API.Common;
using Cerberus.API.Data;
using Cerberus.API.DTO;
using Cerberus.API.Extensions;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MSFramework.AspNetCore;
using MSFramework.Data;
using MSFramework.Domain;
using MSFramework.Extensions;
using MySql.Data.MySqlClient;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Cerberus.API.Controllers
{
    [ApiController]
    [Route("api/v1.0/users")]
    [Authorize]
    public class UserController : MSFrameworkApiControllerBase
    {
        private readonly IdentityDbContext _dbContext;
        private readonly UserManager<User> _userManager;
        private readonly CerberusOptions _options;

        public UserController(CerberusOptions options,
            UserManager<User> userManager,
            IdentityDbContext dbContext, IMSFrameworkSession session,
            ILogger<UserController> logger) : base(session, logger)
        {
            _options = options;
            _dbContext = dbContext;
            _userManager = userManager;
        }

        [HttpGet("{userId}/services/{serviceId}/permissions")]
        [AllowAnonymous]
        public async Task<IEnumerable<dynamic>> GetPermissionsAsync(string userId, string serviceId)
        {
            userId.NotNullOrWhiteSpace(nameof(userId));
            serviceId.NotNullOrWhiteSpace(nameof(serviceId));

            if (userId != Session.UserId)
            {
                throw new ApplicationException("Access dined");
            }

            var param = new {UserId = userId, ServiceId = serviceId};
            await using var conn = new MySqlConnection(_options.ConnectionString);
            var permissions = (await conn.QueryAsync<ListPermissionDTO>(
                $"SELECT b.Id, b.Name, b.Type, b.Module, b.Identification, b.Description FROM UserPermission a LEFT JOIN Permission AS b ON a.PermissionId = b.Id WHERE b.ServiceId = @ServiceId AND a.UserId = @UserId AND b.Expired = false",
                param)).ToList();
            permissions.AddRange(await conn.QueryAsync<ListPermissionDTO>(
                $@"SELECT b.Id, b.Name, b.Type, b.Module, b.Identification, b.Description FROM RolePermission AS a LEFT JOIN Permission AS b ON a.PermissionId = b.Id WHERE a.RoleId IN (
				SELECT Id FROM UserRole AS c LEFT JOIN Role AS d ON c.RoleId = d.Id WHERE c.UserId = @UserId
					) AND b.Expired = false AND b.ServiceId = @ServiceId", param));
            var dict = new Dictionary<string, dynamic>();
            foreach (var x in permissions)
            {
                if (!dict.ContainsKey(x.Id))
                {
                    dict.Add(x.Id, new
                    {
                        x.Id,
                        x.Service,
                        x.Module,
                        x.Name,
                        x.Identification,
                        x.Description,
                        Type = Enum.Parse<PermissionType>(x.Type).ToString()
                    });
                }
            }

            return dict.Values;
        }

        [HttpHead("{userId}/services/{serviceId}/permissions")]
        [AllowAnonymous]
        public async Task<IActionResult> HasPermissionAsync(string userId, string serviceId, string identification)
        {
            userId.NotNullOrWhiteSpace(nameof(userId));
            serviceId.NotNullOrWhiteSpace(nameof(serviceId));
            identification.NotNullOrWhiteSpace(nameof(identification));

            var param = new {UserId = userId, Identification = identification.ToLower()};
            await using var conn = new MySqlConnection(_options.ConnectionString);
            var id = await conn.QueryFirstOrDefaultAsync<string>(
                $"SELECT a.PermissionId FROM UserPermission a LEFT JOIN Permission AS b ON a.PermissionId = b.Id WHERE a.UserId = @UserId AND b.Identification=@Identification",
                param);
            var hasPermission = !string.IsNullOrWhiteSpace(id);
            if (!hasPermission)
            {
                id = await conn.QueryFirstOrDefaultAsync<string>(
                    $@"SELECT a.PermissionId FROM RolePermission AS a LEFT JOIN Permission AS b ON a.PermissionId = b.Id WHERE a.RoleId IN (
				SELECT Id FROM UserRole AS c LEFT JOIN Role AS d ON c.RoleId = d.Id WHERE c.UserId = @UserId
					) AND b.Identification=@Identification", param);
                if (string.IsNullOrWhiteSpace(id))
                {
                    return NotFound();
                }
                else
                {
                    return Ok();
                }
            }
            else
            {
                return Ok();
            }
        }

        #region Permission

        [HttpGet("{userId}/permissions")]
        [Authorize(Roles = "cerberus-admin,admin")]
        public async Task<UserPermissionsDTO> GetPermissionsAsync(string userId)
        {
            var entity = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (entity == null)
            {
                throw new ArgumentException("User not exists");
            }

            var claims = await _userManager.GetClaimsAsync(entity);
            var givenName = claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.GivenName)?.Value;
            var familyName = claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.FamilyName)?.Value;
            var result = new UserPermissionsDTO
            {
                UserName = entity.UserName,
                Name = $"{familyName}{givenName}",
                Email = entity.Email,
                Services = new List<ServicePermissionsDTO>()
            };

            var permissions = await _dbContext.Permissions.Include(x => x.Service).ToListAsync();

            var dict = await _dbContext.UserPermissions.Where(x => x.UserId == userId)
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

        [HttpPost("{userId}/permissions/{permissionId}")]
        [Authorize(Roles = "cerberus-admin,admin")]
        public async Task<bool> AddPermission(string userId, string permissionId)
        {
            userId.NotNullOrWhiteSpace(nameof(userId));
            permissionId.NotNullOrWhiteSpace(nameof(permissionId));
            var entity = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (entity == null)
            {
                throw new ArgumentException("User not exists");
            }

            var permission = await _dbContext.Permissions.FirstOrDefaultAsync(x => x.Id == permissionId);
            if (permission == null)
            {
                throw new ArgumentException("Permission not exists");
            }

            var userPermission = new UserPermission(userId, permissionId);
            await _dbContext.UserPermissions.AddAsync(userPermission);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        [HttpDelete("{userId}/permissions/{permissionId}")]
        [Authorize(Roles = "cerberus-admin,admin")]
        public async Task<bool> DeletePermission(string userId, string permissionId)
        {
            userId.NotNullOrWhiteSpace(nameof(userId));
            permissionId.NotNullOrWhiteSpace(nameof(permissionId));
            var entity =
                await _dbContext.UserPermissions.FirstOrDefaultAsync(x =>
                    x.UserId == userId && x.PermissionId == permissionId);
            if (entity == null)
            {
                throw new ArgumentException("User not exists");
            }

            _dbContext.UserPermissions.Remove(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Role

        [HttpGet("{userId}/roles")]
        [Authorize(Roles = "cerberus-admin,admin")]
        public async Task<UserRolesDTO> GetRolesAsync(string userId)
        {
            var entity = await _userManager.FindByIdAsync(userId);
            if (entity == null)
            {
                throw new ArgumentException("User not exists");
            }

            var claims = await _userManager.GetClaimsAsync(entity);
            var givenName = claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.GivenName)?.Value;
            var familyName = claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.FamilyName)?.Value;
            var result = new UserRolesDTO
            {
                UserName = entity.UserName,
                Name = $"{familyName}{givenName}",
                Email = entity.Email,
                Groups = new List<GroupRoleDTO>()
            };
            var roles = await _dbContext.Roles.ToListAsync();

            var dict = await _dbContext.UserRoles.Where(x => x.UserId == userId)
                .ToDictionaryAsync(x => x.RoleId, x => x);
            var groups = roles.GroupBy(x => x.Type);

            foreach (var group in groups)
            {
                var dto = new GroupRoleDTO {Roles = new List<GrantRoleDTO>(), Type = group.Key};
                foreach (var permission in group)
                {
                    dto.Roles.Add(new GrantRoleDTO
                    {
                        Name = permission.Name, Id = permission.Id, InRole = dict.ContainsKey(permission.Id)
                    });
                }

                result.Groups.Add(dto);
            }

            return result;
        }

        [HttpPost("{userId}/roles/{roleId}")]
        [Authorize(Roles = "cerberus-admin,admin")]
        public async Task<bool> AddRoleAsync(string userId, string roleId)
        {
            userId.NotNullOrWhiteSpace(nameof(userId));
            roleId.NotNullOrWhiteSpace(nameof(roleId));
            var entity = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (entity == null)
            {
                throw new ArgumentException("User not exists");
            }

            var role = await _dbContext.Roles.FirstOrDefaultAsync(x => x.Id == roleId);
            if (role == null)
            {
                throw new ArgumentException("Permission not exists");
            }

            var userRole = new IdentityUserRole<string> {RoleId = roleId, UserId = userId};
            await _dbContext.UserRoles.AddAsync(userRole);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        [HttpDelete("{userId}/roles/{roleId}")]
        [Authorize(Roles = "cerberus-admin,admin")]
        public async Task<bool> DeleteRoleAsync(string userId, string roleId)
        {
            userId.NotNullOrWhiteSpace(nameof(userId));
            roleId.NotNullOrWhiteSpace(nameof(roleId));
            var entity =
                await _dbContext.UserRoles.FirstOrDefaultAsync(x =>
                    x.UserId == userId && x.RoleId == roleId);
            if (entity == null)
            {
                throw new ArgumentException("User not exists");
            }

            _dbContext.UserRoles.Remove(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        #endregion

        #region User

        [HttpGet]
        public async Task<dynamic> QueryAsync(string keyword, string source,
            int page,
            int limit)
        {
            if (!(User.IsInRole("cerberus-admin") || User.IsInRole("admin")))
            {
                var role1 = $"{source.Trim()}-admin";
                var role2 = $"{source.Trim()}-user-admin";
                if (!(User.IsInRole(role1) || User.IsInRole(role2)))
                {
                    return StatusCode(401);
                }
            }

            PagedQueryResult<User> result;
            if (string.IsNullOrWhiteSpace(keyword))
            {
                if (string.IsNullOrWhiteSpace(source))
                {
                    result = await _dbContext.Users.Include(x => x.UserClaims)
                        .PagedQueryAsync(page, limit,
                            x => x.IsDeleted == false,
                            new OrderCondition<User, DateTimeOffset?>(x => x.LastModificationTime, true));
                }
                else
                {
                    result = await _dbContext.Users.Include(x => x.UserClaims).PagedQueryAsync(page, limit,
                        x => x.IsDeleted == false && x.Source == source.Trim(),
                        new OrderCondition<User, DateTimeOffset?>(x => x.LastModificationTime, true));
                }
            }
            else
            {
                keyword = keyword.Trim();
                if (string.IsNullOrWhiteSpace(source))
                {
                    result = await _dbContext.Users.Include(x => x.UserClaims).PagedQueryAsync(page, limit,
                        x => x.IsDeleted == false && (x.UserName.Contains(keyword) || x.Email.Contains(keyword) ||
                                                      x.PhoneNumber.Contains(keyword)),
                        new OrderCondition<User, DateTimeOffset?>(x => x.LastModificationTime, true));
                }
                else
                {
                    source = source.Trim();
                    result = await _dbContext.Users.Include(x => x.UserClaims).PagedQueryAsync(page, limit,
                        x => x.Source == source && x.IsDeleted == false &&
                             (x.UserName.Contains(keyword) || x.Email.Contains(keyword) ||
                              x.PhoneNumber.Contains(keyword)),
                        new OrderCondition<User, DateTimeOffset?>(x => x.LastModificationTime, true));
                }
            }

            var output = new PagedQueryResult<ListUserDTO>()
            {
                Page = result.Page, Limit = result.Limit, Count = result.Count, Data = new List<ListUserDTO>()
            };
            foreach (var user in result.Data)
            {
                var familyName = user.UserClaims.FirstOrDefault(x => x.ClaimType == JwtRegisteredClaimNames.FamilyName);
                var givenName = user.UserClaims.FirstOrDefault(x => x.ClaimType == JwtRegisteredClaimNames.GivenName);
                var title = user.UserClaims.FirstOrDefault(x => x.ClaimType == "title");
                var company = user.UserClaims.FirstOrDefault(x => x.ClaimType == "company");
                output.Data.Add(new ListUserDTO
                {
                    Id = user.Id,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Source = user.Source,
                    FamilyName = familyName == null ? string.Empty : familyName.ClaimValue,
                    GivenName = givenName == null ? string.Empty : givenName.ClaimValue,
                    UserName = user.UserName,
                    Title = title == null ? string.Empty : title.ClaimValue,
                    Company = company == null ? string.Empty : company.ClaimValue,
                    Enabled = user.Enabled
                });
            }

            return output;
        }

        [HttpPost]
        [Authorize(Roles = "cerberus-admin,admin")]
        public async Task<bool> CreateAsync([FromBody] CreateUserDTO dto)
        {
            var entity = new User
            {
                Email = dto.Email, EmailConfirmed = true, UserName = dto.Email,
                Source = dto.Source
            };
            entity.SetCreationAudited(Session.UserId, Session.UserName);
            entity.SetModificationAudited(null, null, DateTimeOffset.Now);

            var result = await _userManager.CreateAsync(entity, dto.Password);
            if (result.Succeeded)
            {
                return true;
            }
            else
            {
                throw new ApplicationException(string.Join(", ", result.Errors.Select(x => x.Description)));
            }
        }

        [HttpDelete("{userId}")]
        [Authorize(Roles = "cerberus-admin,admin")]
        public async Task<bool> DeleteAsync(string userId)
        {
            var entity = await _userManager.FindByIdAsync(userId);
            if (entity == null)
            {
                throw new ArgumentException("User not exists");
            }

            var options = HttpContext.RequestServices.GetRequiredService<CerberusOptions>();
            if (entity.Email == options.AdminEmail)
            {
                throw new ApplicationException("Access dined");
            }

            entity.Delete(Session.UserId, Session.UserName);
            _dbContext.Update(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        [HttpGet("{userId}")]
        public async Task<UserDTO> GetAsync(string userId)
        {
            var entity = await _userManager.FindByIdAsync(userId);
            if (entity == null)
            {
                throw new ArgumentException("User not exists");
            }

            var claims = await _userManager.GetClaimsAsync(entity);
            var sex = claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Gender)?.Value;
            var output = new UserDTO
            {
                Id = entity.Id,
                Email = entity.Email,
                PhoneNumber = entity.PhoneNumber,
                UserName = entity.UserName,
                Telephone = claims.FirstOrDefault(x => x.Type == "telephone")?.Value,
                Bio = claims.FirstOrDefault(x => x.Type == "bio")?.Value,
                Department = claims.FirstOrDefault(x => x.Type == "department")?.Value,
                Fax = claims.FirstOrDefault(x => x.Type == "fax")?.Value,
                Title = claims.FirstOrDefault(x => x.Type == "title")?.Value,
                GivenName = claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.GivenName)?.Value,
                FamilyName = claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.FamilyName)?.Value,
                NickName = claims.FirstOrDefault(x => x.Type == "nickname")?.Value,
                Website = claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Website)?.Value,
                Company = claims.FirstOrDefault(x => x.Type == "company")?.Value,
                Location = claims.FirstOrDefault(x => x.Type == "location")?.Value,
                Sex = string.IsNullOrWhiteSpace(sex) ? "Unknown" : sex,
                Source = entity.Source,
                Enabled = entity.Enabled
            };
            var birthday = claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Birthdate)?.Value;
            if (!string.IsNullOrWhiteSpace(birthday))
            {
                output.Birthday = birthday;
            }

            return output;
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "cerberus-admin,admin")]
        public async Task<bool> UpdateAsync(string id, [FromBody] UserDTO dto)
        {
            var entity = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
            {
                throw new ArgumentException("User not exists");
            }

            entity.SetModificationAudited(Session.UserId, Session.UserName);
            entity.Modify(dto.UserName, dto.Email, dto.PhoneNumber, dto.Source);

            _dbContext.Users.Update(entity);
            DateTime.TryParse(dto.Birthday, out var birthday);
            await _dbContext.AddOrUpdateUserClaimsAsync(id,
                new Dictionary<string, string>
                {
                    {"bio", dto.Bio},
                    {"company", dto.Company},
                    {"department", dto.Department},
                    {"fax", dto.Fax},
                    {"location", dto.Location},
                    {"telephone", dto.Telephone},
                    {"title", dto.Title},
                    {"nickname", dto.NickName},
                    {
                        JwtRegisteredClaimNames.Birthdate,
                        birthday == DateTime.MinValue ? "" : birthday.ToString("yyyy-MM-dd")
                    },
                    {JwtRegisteredClaimNames.Website, dto.Website},
                    {JwtRegisteredClaimNames.FamilyName, dto.FamilyName},
                    {JwtRegisteredClaimNames.GivenName, dto.GivenName},
                    {JwtRegisteredClaimNames.Gender, dto.Sex}
                });

            return true;
        }

        [HttpPut("{id}/disable")]
        [Authorize(Roles = "cerberus-admin,admin")]
        public async Task<bool> DisableAsync(string id)
        {
            var entity = await _userManager.FindByIdAsync(id);
            if (entity == null)
            {
                throw new ArgumentException("User not exists");
            }

            entity.SetModificationAudited(Session.UserId, Session.UserName);
            await _userManager.SetLockoutEnabledAsync(entity, true);
            await _userManager.SetLockoutEndDateAsync(entity, DateTimeOffset.MaxValue);
            return true;
        }

        [HttpPut("{id}/enable")]
        [Authorize(Roles = "cerberus-admin,admin")]
        public async Task<bool> EnableAsync(string id)
        {
            var entity = await _userManager.FindByIdAsync(id);
            if (entity == null)
            {
                throw new ArgumentException("User not exists");
            }

            entity.SetModificationAudited(Session.UserId, Session.UserName);
            await _userManager.SetLockoutEnabledAsync(entity, false);
            await _userManager.SetLockoutEndDateAsync(entity, null);
            return true;
        }

        #endregion
    }
}