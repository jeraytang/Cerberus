using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace IdentityServer4.SecurityTokenService.Extensions
{
	public static class IdentityOptionsExtensions
	{
		public static IdentityOptions GetIdentityOptions(this IConfiguration configuration)
		{
			var con = new NamedConfigureFromConfigurationOptions<IdentityOptions>(
				Options.DefaultName,
				configuration.GetSection("Identity"));
			var optionsFactory =
				new OptionsFactory<IdentityOptions>(new[] {con}, new IPostConfigureOptions<IdentityOptions>[0]);
			return optionsFactory.Create(Options.DefaultName);
		}
	}
}
