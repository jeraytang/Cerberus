using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cerberus.API.Common;
using Cerberus.API.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cerberus.API.Extensions
{
	public static class DbContextExtensions
	{
		public static async Task AddOrUpdateUserClaimsAsync(this IdentityDbContext dbContext, string userId,
			Dictionary<string, string> claims)
		{
			var oldClaimDict = dbContext.UserClaims.Where(x => x.UserId == userId)
				.ToDictionary(x => x.ClaimType, x => x);
			foreach (var kv in claims)
			{
				if (string.IsNullOrWhiteSpace(kv.Value))
				{
					if (oldClaimDict.ContainsKey(kv.Key))
					{
						dbContext.UserClaims.Remove(oldClaimDict[kv.Key]);
					}
					else
					{
						continue;
					}
				}
				else
				{
					if (!oldClaimDict.ContainsKey(kv.Key))
					{
						await dbContext.UserClaims.AddAsync(new IdentityUserClaim<string>
						{
							UserId = userId, ClaimType = kv.Key, ClaimValue = kv.Value
						});
					}
					else
					{
						oldClaimDict[kv.Key].ClaimValue = kv.Value;
					}
				}
			}

			foreach (var kv in oldClaimDict)
			{
				if (!claims.ContainsKey(kv.Key))
				{
					dbContext.UserClaims.Remove(oldClaimDict[kv.Key]);
				}
			}

			await dbContext.SaveChangesAsync();
		}

		public static IApplicationBuilder Migrate(this IApplicationBuilder builder)
		{
			using var scope = builder.ApplicationServices.CreateScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
			var migrations = dbContext.Database.GetPendingMigrations().ToArray();
			if (migrations.Length > 0)
			{
				dbContext.Database.Migrate();
				var logger = dbContext.GetService<ILoggerFactory>()
					.CreateLogger("DbContextExtensions");
				logger.LogInformation($"已提交{migrations.Length}条挂起的迁移记录：{string.Join(';', migrations)}");
			}

			var options = scope.ServiceProvider.GetRequiredService<CerberusOptions>();
			var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
			if (!roleManager.RoleExistsAsync("admin").Result)
			{
				var role = new Role("admin", "Role");
				role.SetCreationAudited("System", "System");
				role.SetModificationAudited(null, null, DateTimeOffset.Now);
				roleManager.CreateAsync(role).GetAwaiter().GetResult();
			}

			if (!roleManager.RoleExistsAsync("cerberus-admin").Result)
			{
				var role = new Role("cerberus-admin", "Role");
				role.SetCreationAudited("System", "System");
				role.SetModificationAudited(null, null, DateTimeOffset.Now);
				roleManager.CreateAsync(role).GetAwaiter().GetResult();
			}

			var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
			if (!string.IsNullOrWhiteSpace(options.AdminEmail))
			{
				var user = userManager.FindByEmailAsync(options.AdminEmail).Result;
				if (user == null)
				{
					user = new User {UserName = "admin", Email = options.AdminEmail, EmailConfirmed = true};
					user.SetCreationAudited("System", "System");
					user.SetModificationAudited(null, null, DateTimeOffset.Now);

					var identityResult = userManager
						.CreateAsync(user, options.AdminPassword).Result;
					if (!identityResult.Succeeded)
					{
						throw new Exception(string.Join(", ", identityResult.Errors.Select(x => x.Description)));
					}
				}

				var roles = userManager.GetRolesAsync(user).Result;
				if (!roles.Contains("cerberus-admin"))
				{
					userManager.AddToRoleAsync(user, "cerberus-admin").GetAwaiter().GetResult();
				}
			}

			if (options.TestData == "true" && userManager.FindByEmailAsync("test0@cerberus.com").Result == null)
			{
				for (var i = 0; i < 5; i++)
				{
					var user = new User {UserName = $"test{i}", Email = $"test{i}@cerberus.com", EmailConfirmed = true};
					user.SetCreationAudited("System", "System");
					user.SetModificationAudited(null, null, DateTimeOffset.Now);

					var identityResult = userManager
						.CreateAsync(user, options.AdminPassword).Result;
					if (!identityResult.Succeeded)
					{
						throw new Exception(string.Join(", ", identityResult.Errors.Select(x => x.Description)));
					}

					var role = new Role($"role{i}", "Role");
					role.SetCreationAudited("System", "System");
					role.SetModificationAudited(null, null, DateTimeOffset.Now);
					roleManager.CreateAsync(role).GetAwaiter().GetResult();
				}
			}

			return builder;
		}
	}
}
