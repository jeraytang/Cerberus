using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.SecurityTokenService.Data;
using IdentityServer4.Storage.Adapter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.SecurityTokenService.Extensions
{
	public static class DbContextExtensions
	{
		public static async Task AddOrUpdateClaimsAsync(
			this IdentityServer4.SecurityTokenService.Data.IdentityDbContext dbContext, string id,
			Dictionary<string, string> claims)
		{
			var oldClaimDict = dbContext.UserClaims.Where(x => x.UserId == id)
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
							UserId = id, ClaimType = kv.Key, ClaimValue = kv.Value
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

		public static IApplicationBuilder MigrateIdentity(this IApplicationBuilder builder)
		{
			var configuration = builder.ApplicationServices.GetRequiredService<IConfiguration>();
			using var scope = builder.ApplicationServices.CreateScope();

			// 只有自部署 identity 时使用
			if (string.IsNullOrWhiteSpace(configuration["IdentityConnectionString"]))
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
				var migrations = dbContext.Database.GetPendingMigrations().ToArray();
				if (migrations.Length > 0)
				{
					dbContext.Database.Migrate();
					var logger = dbContext.GetService<ILoggerFactory>()
						.CreateLogger("DbContextExtensions");
					logger.LogInformation($"已提交{migrations.Length}条挂起的迁移记录：{string.Join(';', migrations)}");
				}

				var options = scope.ServiceProvider.GetRequiredService<IdentityServerOptions>();
				if (!string.IsNullOrWhiteSpace(options.AdminEmail))
				{
					var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
					if (!roleManager.RoleExistsAsync("admin").Result)
					{
						roleManager.CreateAsync(new IdentityRole("admin")).GetAwaiter().GetResult();
					}

					var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
					var user = userManager.FindByEmailAsync(options.AdminEmail).Result;
					if (user == null)
					{
						user = new IdentityUser {UserName = "admin", Email = options.AdminEmail, EmailConfirmed = true};
						var identityResult = userManager
							.CreateAsync(user, options.AdminPassword).Result;
						if (!identityResult.Succeeded)
						{
							throw new Exception(string.Join(", ", identityResult.Errors.Select(x => x.Description)));
						}
					}

					var roles = userManager.GetRolesAsync(user).Result;
					if (!roles.Contains("admin"))
					{
						userManager.AddToRoleAsync(user, "admin").GetAwaiter().GetResult();
					}
				}

				return builder;
			}

			return builder;
		}
	}
}
