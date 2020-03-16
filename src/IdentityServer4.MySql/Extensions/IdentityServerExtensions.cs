using System;
using System.Collections.Generic;
using AutoMapper;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Storage.Adapter;
using IdentityServer4.Storage.Adapter.Repository;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Client = IdentityServer4.Storage.Adapter.Entities.Client;
using IdentityResource = IdentityServer4.Storage.Adapter.Entities.IdentityResource;

namespace IdentityServer4.MySql.Extensions
{
	public class Role : IdentityServer4.Models.IdentityResource
	{
		public Role()
		{
			this.Name = "role";
			this.DisplayName = "Your role";
			this.Emphasize = true;
			this.UserClaims = new[] {"role"};
		}
	}

	public static class IdentityServerExtensions
	{
		public static IEnumerable<IdentityServer4.Models.IdentityResource> Ids =>
			new List<IdentityServer4.Models.IdentityResource>
			{
				new IdentityResources.OpenId(),
				new IdentityResources.Profile(),
				new IdentityResources.Email(),
				new IdentityResources.Phone(),
				new Role()
			};

		public static IApplicationBuilder Init(this IApplicationBuilder app)
		{
			using var scope = app.ApplicationServices.CreateScope();
			var options = scope.ServiceProvider.GetRequiredService<IdentityServerOptions>();
			var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

			var identityResourceRepository =
				scope.ServiceProvider.GetRequiredService<IIdentityResourceRepository>();
			var identities = mapper.Map<IdentityResource[]>(Ids);
			foreach (var id in identities)
			{
				var exits = identityResourceRepository.ExistsAsync(id.Name).GetAwaiter().GetResult();
				if (!exits)
				{
					id.SetCreationAudited("System", "System");
					identityResourceRepository.AddAsync(id);
				}
			}

			if (options.EnableAdmin)
			{
				if (string.IsNullOrWhiteSpace(options.ClientId)
				    || string.IsNullOrWhiteSpace(options.RedirectUri)
				    || string.IsNullOrWhiteSpace(options.ClientSecret))
				{
					throw new ArgumentException("ClientId & RedirectUris & ClientSecrets should not be null");
				}
				else
				{
					var clientStore = scope.ServiceProvider.GetRequiredService<IClientStore>();

					var client = clientStore.FindClientByIdAsync(options.ClientId).GetAwaiter().GetResult();
					if (client == null)
					{
						var clientRepository = scope.ServiceProvider.GetRequiredService<IClientRepository>();

						var adminClient = mapper.Map<Client>(new IdentityServer4.Models.Client
						{
							ClientId = options.ClientId,
							ClientName = "IdentityServer4 admin",
							AllowAccessTokensViaBrowser = false,
							RequireClientSecret = true,
							AllowedGrantTypes = new[] {GrantType.Implicit},
							RedirectUris = new[] {options.RedirectUri},
							AllowedScopes = new[] {"openid", "profile", "role"},
							PostLogoutRedirectUris = new[] {options.PostLogoutRedirectUri},
							ClientSecrets = new[] {new Secret(options.ClientSecret.ToSha256(), "default")},
							AllowRememberConsent = true,
							RequireConsent = false
						});
						adminClient.SetCreationAudited("System", "System");
						clientRepository.AddAsync(adminClient).GetAwaiter().GetResult();
					}
				}
			}

			return app;
		}
	}
}
