using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Cerberus.API.Data
{
    public class IdentityDbContext : IdentityDbContext<User, Role, string>
    {
        public DbSet<Permission> Permissions { get; set; }

        public DbSet<UserPermission> UserPermissions { get; set; }

        public DbSet<RolePermission> RolePermissions { get; set; }

        public DbSet<Service> Services { get; set; }

        protected IdentityDbContext()
        {
        }

        public IdentityDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserPermission>(b =>
            {
                b.ToTable("UserPermission");
                b.HasKey(x => new {x.PermissionId, x.UserId});
            });
            builder.Entity<RolePermission>(b =>
            {
                b.ToTable("RolePermission");
                b.HasKey(x => new {x.PermissionId, x.RoleId});
            });

            builder.Entity<Permission>(b =>
            {
                b.ToTable("Permission");
                b.Property(x => x.Id).HasMaxLength(36);
                b.HasIndex(x => x.ServiceId);
                b.HasIndex(x => x.LastModificationTime);
                b.HasIndex(x => x.CreationTime);
                b.HasIndex("ServiceId", "Identification").IsUnique();
            });

            builder.Entity<User>(b =>
            {
                b.ToTable("User");

                b.Property(x => x.ConcurrencyStamp).HasMaxLength(36);
                b.Property(x => x.PasswordHash).HasMaxLength(512);
                b.Property(x => x.SecurityStamp).HasMaxLength(36);
                b.Property(x => x.PhoneNumber).HasMaxLength(50);

                b.HasIndex(x => x.PhoneNumber);
                b.HasIndex(x => x.Email);
                b.HasIndex(x => x.Source);
                b.HasIndex(x => x.LastModificationTime);
                b.HasIndex(x => x.CreationTime);

               
                b.HasMany(x=>x.UserClaims).WithOne().HasForeignKey(uc => uc.UserId).IsRequired();
            });
            builder.Entity<Role>(b =>
            {
                b.ToTable("Role");

                b.HasIndex(x => x.LastModificationTime);
                b.HasIndex(x => x.CreationTime);
                // b.HasMany<RolePermission>().WithOne().HasForeignKey(x => x.RoleId).IsRequired();
            });

            builder.Entity<Service>(b =>
            {
                b.HasIndex(x => x.LastModificationTime);
                b.HasIndex(x => x.CreationTime);
                b.HasIndex(x => x.Name).IsUnique();
                b.ToTable("Service");
            });
            builder.Entity<IdentityUserLogin<string>>(b => { b.ToTable("UserLogin"); });
            builder.Entity<IdentityUserClaim<string>>(b =>
            {
                b.ToTable("UserClaim");
                b.Property(x => x.ClaimType).HasMaxLength(255);
            });
            builder.Entity<IdentityUserRole<string>>(b => { b.ToTable("UserRole"); });
            builder.Entity<IdentityRoleClaim<string>>(b => { b.ToTable("RoleClaim"); });
            builder.Entity<IdentityUserToken<string>>(b => { b.ToTable("UserToken"); });
        }
    }
}