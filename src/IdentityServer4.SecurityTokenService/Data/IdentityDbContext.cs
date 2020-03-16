using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer4.SecurityTokenService.Data
{
	public class IdentityDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
	{
		public IdentityDbContext(DbContextOptions options) : base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			builder.Entity<IdentityUser>().ToTable("User");
			builder.Entity<IdentityRole>().ToTable("Role");
			builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogin");
			builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaim");
			builder.Entity<IdentityUserRole<string>>().ToTable("UserRole");
			builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaim");
			builder.Entity<IdentityUserToken<string>>().ToTable("UserToken");
		}
	}
}
