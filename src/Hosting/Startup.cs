using AutoMapper;
using IdentityServer4.MySql;
using IdentityServer4.MySql.Services;
using IdentityServer4.Storage.Adapter.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ApiResource = IdentityServer4.Storage.Adapter.Entities.ApiResource;
using Client = IdentityServer4.Storage.Adapter.Entities.Client;

namespace Hosting
{
	public class Startup
	{
		public void ConfigureServices(IServiceCollection services)
		{
			// uncomment, if you want to add an MVC-based UI
			//services.AddControllersWithViews();
			var builder = services.AddIdentityServer().AddMySqlStore();
			builder.AddProfileService<ProfileService>();
			builder.AddDeveloperSigningCredential();
		}

		public void Configure(IApplicationBuilder app)
		{
			app.UseMySqlIdentityServer();

			using var scope = app.ApplicationServices.CreateScope();
			var clientRepository = scope.ServiceProvider.GetRequiredService<IClientRepository>();
			var count = clientRepository.CountAsync().Result;
			if (count == 0)
			{
				var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
				var clients = mapper.Map<Client[]>(Config.Clients);
				clientRepository.AddAsync(clients).GetAwaiter().GetResult();

				var apiResourceRepository = scope.ServiceProvider.GetRequiredService<IApiResourceRepository>();
				var apis = mapper.Map<ApiResource[]>(Config.Apis);
				apiResourceRepository.AddAsync(apis).GetAwaiter().GetResult();

				var identityResourceRepository = scope.ServiceProvider.GetRequiredService<IIdentityResourceRepository>();
				var identities = mapper.Map<IdentityServer4.Storage.Adapter.Entities.IdentityResource[]>(Config.Ids);
				foreach (var identity in identities)
				{
					identityResourceRepository.AddAsync(identity).GetAwaiter().GetResult();
				}
			}

			app.UseDeveloperExceptionPage();

			// uncomment if you want to add MVC
			//app.UseStaticFiles();
			//app.UseRouting();

			app.UseIdentityServer();

			// uncomment, if you want to add MVC-based
			//app.UseAuthorization();
			//app.UseEndpoints(endpoints =>
			//{
			//    endpoints.MapDefaultControllerRoute();
			//});
		}
	}
}
