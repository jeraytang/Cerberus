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
            var task = Task.Factory.StartNew(async () =>
            {
                var logger = builder.ApplicationServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("DbContextExtensions");
                try
                {
                    using var scope = builder.ApplicationServices.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
                    var migrations = dbContext.Database.GetPendingMigrations().ToArray();
                    if (migrations.Length > 0)
                    {
                        dbContext.Database.Migrate();

                        logger.LogInformation($"已提交{migrations.Length}条挂起的迁移记录：{string.Join(';', migrations)}");
                    }

                    var options = scope.ServiceProvider.GetRequiredService<CerberusOptions>();
                    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
                    if (!await roleManager.RoleExistsAsync("admin"))
                    {
                        var role = new Role("admin", "Role");
                        role.SetCreationAudited("System", "System");
                        role.SetModificationAudited(null, null, DateTimeOffset.Now);
                        await roleManager.CreateAsync(role);
                    }

                    if (!await roleManager.RoleExistsAsync("cerberus-admin"))
                    {
                        var role = new Role("cerberus-admin", "Role");
                        role.SetCreationAudited("System", "System");
                        role.SetModificationAudited(null, null, DateTimeOffset.Now);
                        await roleManager.CreateAsync(role);
                    }

                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                    if (!string.IsNullOrWhiteSpace(options.AdminEmail))
                    {
                        var user = await userManager.Users.FirstOrDefaultAsync(x => x.UserName == options.AdminEmail);
                        if (user == null)
                        {
                            user = new User {UserName = "admin", Email = options.AdminEmail, EmailConfirmed = true};
                            user.SetCreationAudited("System", "System");
                            user.SetModificationAudited(null, null, DateTimeOffset.Now);

                            var identityResult = await userManager
                                .CreateAsync(user, options.AdminPassword);
                            if (!identityResult.Succeeded)
                            {
                                throw new Exception(string.Join(", ",
                                    identityResult.Errors.Select(x => x.Description)));
                            }
                        }

                        var roles = await userManager.GetRolesAsync(user);
                        if (!roles.Contains("cerberus-admin"))
                        {
                            await userManager.AddToRoleAsync(user, "cerberus-admin");
                        }
                    }

                    if (options.TestData == "true" &&
                        !await userManager.Users.AnyAsync(x => x.Email == "test0@cerberus.com"))
                    {
                        for (var i = 0; i < 5; i++)
                        {
                            var user = new User
                                {UserName = $"test{i}", Email = $"test{i}@cerberus.com", EmailConfirmed = true};
                            user.SetCreationAudited("System", "System");
                            user.SetModificationAudited(null, null, DateTimeOffset.Now);

                            var identityResult = await userManager
                                .CreateAsync(user, options.AdminPassword);
                            if (!identityResult.Succeeded)
                            {
                                throw new Exception(string.Join(", ",
                                    identityResult.Errors.Select(x => x.Description)));
                            }

                            var role = new Role($"role{i}", "Role");
                            role.SetCreationAudited("System", "System");
                            role.SetModificationAudited(null, null, DateTimeOffset.Now);
                            await roleManager.CreateAsync(role);
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.LogError(e.ToString());
                    throw;
                }
            });
            Task.WaitAll(task);
            return builder;
        }
    }
}