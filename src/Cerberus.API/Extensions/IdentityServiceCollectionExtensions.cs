using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cerberus.API.Extensions
{
	public static class IdentityServiceCollectionExtensions
	{
		public static IdentityBuilder AddIdentityCore<TUser, TRole>(
			this IServiceCollection services,
			Action<IdentityOptions> setupAction)
			where TUser : class
			where TRole : class
		{
			services.AddHttpContextAccessor();
			services.TryAddScoped<IUserValidator<TUser>, UserValidator<TUser>>();
			services.TryAddScoped<IPasswordValidator<TUser>, PasswordValidator<TUser>>();
			services.TryAddScoped<IPasswordHasher<TUser>, PasswordHasher<TUser>>();
			services.TryAddScoped<ILookupNormalizer, UpperInvariantLookupNormalizer>();
			services.TryAddScoped<IRoleValidator<TRole>, RoleValidator<TRole>>();
			services.TryAddScoped<IdentityErrorDescriber>();
			services.TryAddScoped<ISecurityStampValidator, SecurityStampValidator<TUser>>();
			services.TryAddScoped<ITwoFactorSecurityStampValidator, TwoFactorSecurityStampValidator<TUser>>();
			services.TryAddScoped<IUserClaimsPrincipalFactory<TUser>, UserClaimsPrincipalFactory<TUser, TRole>>();
			services.TryAddScoped<IUserConfirmation<TUser>, DefaultUserConfirmation<TUser>>();
			services.TryAddScoped<UserManager<TUser>>();
			services.TryAddScoped<SignInManager<TUser>>();
			services.TryAddScoped<RoleManager<TRole>>();
			if (setupAction != null)
			{
				services.Configure(setupAction);
			}

			return new IdentityBuilder(typeof(TUser), typeof(TRole), services);
		}
	}
}
